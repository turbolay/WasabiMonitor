using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBitcoin;
using WabiSabiMonitor.ApplicationCore;
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

namespace WabiSabiMonitor;

public static partial class Program
{
    private static object LockClosure { get; } = new();
    private static CancellationTokenSource CancellationTokenSource;
    private static IServiceProvider ServiceProvider;

    public static async Task Main(string[] args)
    {
        CancellationTokenSource = new CancellationTokenSource();

        HandleClosure();

        var host = CreateHostBuilder(args).Build();

        ServiceProvider = host.Services;
        var applicationCore = host.Services.GetRequiredService<ApplicationCore.ApplicationCore>();

        try
        {
            await applicationCore.Run();
        }
        catch (Exception ex)
        {
            Logger.LogInfo(ex);
        }
        finally
        {
            await TerminateApplicationAsync();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) => ConfigureServices(services));

    public static void ConfigureServices(IServiceCollection services)
    {
        var repositoryPath =
            Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");
        var config = new Config(LoadOrCreateConfigs(), Array.Empty<string>());
        var jsonRpcServerConfig = new JsonRpcServerConfiguration(true, config!.JsonRpcUser,
            config.JsonRpcPassword, config.JsonRpcServerPrefixes);

        services
            .AddHttpClient<ClearnetHttpClient>()
            .ConfigureHttpClient(c => c.BaseAddress = config.GetCoordinatorUri());

        services.AddSingleton<IRoundDataProcessor, RoundDataProcessor>()
            .AddSingleton<IRoundsDataFilter, RoundsDataFilter>()
            .AddSingleton<IAnalyzer, Analyzer>()
            .AddSingleton<IWabiSabiApiRequestHandlerAdapter, WabiSabiApiRequestHandlerAdapter>()
            .AddSingleton<BetterHumanMonitor>()
            .AddSingleton<Scraper>()
            .AddSingleton<PersistentConfig>()
            .AddSingleton<RoundDataReaderService>()
            .AddSingleton<Config>(config)
            //need change
            .AddSingleton(new Dictionary<uint256, RoundDataReaderService.ProcessedRound>())
            .AddSingleton<JsonRpcServerConfiguration>(jsonRpcServerConfig)
            .AddSingleton<FileProcessedRoundRepository>(sp =>
            {
                var roundDataReaderService = sp.GetRequiredService<RoundDataReaderService>();

                return new FileProcessedRoundRepository(repositoryPath, roundDataReaderService);
            })
            .AddSingleton<WabiSabiHttpApiClient>(sp =>
            {
                var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var client = clientFactory.CreateClient();
                
                client.BaseAddress = config.GetCoordinatorUri();

                return new WabiSabiHttpApiClient(new ClearnetHttpClient(client));
            })
            .AddSingleton<JsonRpcServer>((sp) =>
            {
                var roundDataFilter = sp.GetRequiredService<IRoundsDataFilter>();
                var analyzer = sp.GetRequiredService<IAnalyzer>();
                var betterHumanMonitor = sp.GetRequiredService<BetterHumanMonitor>();

                return new JsonRpcServer(
                    new WabiSabiMonitorRpc(roundDataFilter, analyzer, betterHumanMonitor),
                    jsonRpcServerConfig,
                    new TerminateService(TerminateApplicationAsync, () => { })
                );
            })
            .AddSingleton<RpcServerController>(sp =>
            {
                var jsonRpcServer = sp.GetRequiredService<JsonRpcServer>();
                var jsonRpcServerConfiguration = sp.GetRequiredService<JsonRpcServerConfiguration>();

                return new RpcServerController(jsonRpcServer, jsonRpcServerConfiguration);
            })
            .AddSingleton<ApplicationCore.ApplicationCore>();

        ConfigureClients(services, config);
    }


    private static void ConfigureClients(IServiceCollection services, Config config)
    {
        services.AddHttpClient()
            .AddSingleton<IHttpClient, ClearnetHttpClient>();

        services.AddSingleton<IWabiSabiApiRequestHandler>(sp =>
        {
            var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = clientFactory.CreateClient();

            client.BaseAddress = config.GetCoordinatorUri();

            return new WabiSabiHttpApiClient(new ClearnetHttpClient(client));
        });
    }

    private static Task TerminateApplicationAsync()
    {
        Logger.LogInfo("Closing.");

        var repository = ServiceProvider.GetRequiredService<FileProcessedRoundRepository>();

        repository?.SaveToFileSystem();

        var rpcServer = ServiceProvider.GetRequiredService<JsonRpcServer>();
        rpcServer?.Dispose();

        return Task.CompletedTask;
    }

    private static void HandleClosure()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            lock (LockClosure)
            {
                if (!CancellationTokenSource.IsCancellationRequested)
                {
                    CancellationTokenSource.Cancel();
                }
            }
        };

        Console.CancelKeyPress += (sender, e) =>
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