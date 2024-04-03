using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using NBitcoin;
using WabiSabiMonitor.ApplicationCore;
using WabiSabiMonitor.ApplicationCore.Adapters;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Rpc;
using WabiSabiMonitor.ApplicationCore.Utils.Config;
using WabiSabiMonitor.ApplicationCore.Utils.Helpers;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.Rpc;
using WabiSabiMonitor.ApplicationCore.Utils.Services.Terminate;
using WabiSabiMonitor.ApplicationCore.Utils.Tor.Http;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.PostRequests;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client;

// ReSharper disable InconsistentlySynchronizedField

namespace WabiSabiMonitor;

public static class Program
{
    private static object LockClosure { get; } = new();
    private static readonly CancellationTokenSource CancellationTokenSource = new();

    public static async Task Main(string[] args)
    {
        HandleClosure();

        var host = CreateHostBuilder(args).Build();
        var applicationCore = host.Services.GetRequiredService<ApplicationCore.ApplicationCore>();

        try
        {
            await applicationCore.Run(CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            if (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                Logger.LogCritical(ex);
            }
        }
        finally
        {
            await TerminateApplicationAsync(
                host.Services.GetRequiredService<IProcessedRoundRepository>(),
                host.Services.GetRequiredService<IRoundDataReaderService>(),
                host.Services.GetRequiredService<JsonRpcServer>());
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) => ConfigureServices(services));

    public static void ConfigureServices(IServiceCollection services)
    {
        var repositoryPath = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");
        var config = new Config(LoadOrCreateConfigs(), Array.Empty<string>());
        var jsonRpcServerConfig = new JsonRpcServerConfiguration(true, config.JsonRpcUser, config.JsonRpcPassword, config.JsonRpcServerPrefixes);

        services
            .AddHttpClient<IHttpClient, ClearnetHttpClient>()
            .ConfigureHttpClient(c => c.BaseAddress = config.GetCoordinatorUri());

        services.AddSingleton<IRoundDataProcessor, RoundDataProcessor>()
            .AddSingleton<IRoundsDataFilter, RoundsDataFilter>()
            .AddSingleton<IAnalyzer, Analyzer>()
            .AddSingleton<IBetterHumanMonitor, BetterHumanMonitor>()
            .AddSingleton<Scraper>()
            .AddSingleton<PersistentConfig>()
            .AddSingleton<IRoundDataReaderService, RoundDataReaderService>(sp =>
            {
                var fileProcessedRoundRepository = sp.GetRequiredService<IProcessedRoundRepository>();
                var roundsInfo = fileProcessedRoundRepository.ReadFromFileSystem() ??
                                 new Dictionary<uint256, RoundDataReaderService.ProcessedRound>();
                return new RoundDataReaderService(roundsInfo, sp.GetRequiredService<Scraper>());
            })
            .AddSingleton(config)
            .AddSingleton(jsonRpcServerConfig)
            .AddSingleton<IProcessedRoundRepository, FileProcessedRoundRepository>(_ => new FileProcessedRoundRepository(repositoryPath))
            .AddSingleton<IWabiSabiApiRequestHandler, WabiSabiHttpApiClient>(sp =>
            {
                var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var client = clientFactory.CreateClient();

                client.BaseAddress = config.GetCoordinatorUri();

                return new WabiSabiHttpApiClient(new ClearnetHttpClient(client));
            })
            .AddSingleton<IWabiSabiApiRequestHandlerAdapter, WabiSabiApiRequestHandlerAdapter>(sp =>
            {
                var handler = sp.GetRequiredService<IWabiSabiApiRequestHandler>();
                return new WabiSabiApiRequestHandlerAdapter((WabiSabiHttpApiClient)handler);
            })
            .AddSingleton<JsonRpcServer>((sp) =>
            {
                var roundDataFilter = sp.GetRequiredService<IRoundsDataFilter>();
                var analyzer = sp.GetRequiredService<IAnalyzer>();
                var betterHumanMonitor = sp.GetRequiredService<IBetterHumanMonitor>();

                return new JsonRpcServer(
                    new WabiSabiMonitorRpc(roundDataFilter, analyzer, betterHumanMonitor),
                    jsonRpcServerConfig,
                    new TerminateService(() =>
                            TerminateApplicationAsync(
                                sp.GetRequiredService<IProcessedRoundRepository>(),
                                sp.GetRequiredService<IRoundDataReaderService>(),
                                sp.GetRequiredService<JsonRpcServer>()), () => { })
                );
            })
            .AddSingleton<IRpcServerController, RpcServerController>(sp =>
            {
                var jsonRpcServer = sp.GetRequiredService<JsonRpcServer>();
                var jsonRpcServerConfiguration = sp.GetRequiredService<JsonRpcServerConfiguration>();

                return new RpcServerController(jsonRpcServer, jsonRpcServerConfiguration);
            })
            .AddSingleton<ApplicationCore.ApplicationCore>();

        services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
    }

    private static Task TerminateApplicationAsync(IProcessedRoundRepository fileProcessedRoundRepository,
        IRoundDataReaderService roundDataReaderService, JsonRpcServer jsonRpcServer)
    {
        Logger.LogInfo("Closing.");

        fileProcessedRoundRepository.SaveToFileSystem(roundDataReaderService.Rounds);

        jsonRpcServer.Dispose();

        return Task.CompletedTask;
    }

    private static void HandleClosure()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            lock (LockClosure)
            {
                if (!CancellationTokenSource.IsCancellationRequested)
                {
                    CancellationTokenSource.Cancel();
                }
            }
        };

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // Prevent the default Ctrl+C behavior
            lock (LockClosure)
            {
                if (!CancellationTokenSource.IsCancellationRequested)
                {
                    CancellationTokenSource.Cancel();
                }
            }
        };
    }

    private static PersistentConfig LoadOrCreateConfigs()
    {
        Directory.CreateDirectory(Config.DataDir);

        PersistentConfig persistentConfig = new(Path.Combine(Config.DataDir, "Config.json"));
        persistentConfig.LoadFile(createIfMissing: true);

        return persistentConfig;
    }
}