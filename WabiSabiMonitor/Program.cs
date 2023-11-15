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

// ReSharper disable InconsistentlySynchronizedField

namespace WabiSabiMonitor
{
    public static partial class Program
    {
        private static object LockClosure { get; } = new();

        private static readonly CancellationTokenSource CancellationTokenSource = new();


        public static async Task Main(string[] args)
        {
            try
            {
                HandleClosure();

                var host = CreateHostBuilder(args).Build();

                var applicationCore = host.Services.GetRequiredService<ApplicationCore.ApplicationCore>();

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
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<IRoundsDataFilter, RoundsDataFilter>();
                    services.AddSingleton<IAnalyzer, Analyzer>();
                    services.AddSingleton<BetterHumanMonitor>();
                    services.AddSingleton<IWabiSabiApiRequestHandler, WabiSabiHttpApiClient>((sp) =>
                    {
                        var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
                        var client = clientFactory.CreateClient();
                        var config = sp.GetRequiredService<Config>();
                        client.BaseAddress = config.GetCoordinatorUri();

                        return new WabiSabiHttpApiClient(new ClearnetHttpClient(client));
                    });

                    services.AddSingleton<JsonRpcServer>((sp) =>
                    {
                        var jsonRpcServerConfig = sp.GetRequiredService<JsonRpcServerConfiguration>();
                        var roundDataFilter = sp.GetRequiredService<IRoundsDataFilter>();
                        var analyzer = sp.GetRequiredService<IAnalyzer>();
                        var betterHumanMonitor = sp.GetRequiredService<BetterHumanMonitor>();

                        return new JsonRpcServer(
                            new WabiSabiMonitorRpc(roundDataFilter, analyzer, betterHumanMonitor),
                            jsonRpcServerConfig,
                            new TerminateService(TerminateApplicationAsync, () => { })
                        );
                    });

                    services.AddSingleton<RpcServerController>((sp) =>
                    {
                        var jsonRpcServer = sp.GetRequiredService<JsonRpcServer>();
                        var jsonRpcServerConfiguration = sp.GetRequiredService<JsonRpcServerConfiguration>();

                        return new RpcServerController(jsonRpcServer, jsonRpcServerConfiguration);
                    });

                    services.AddSingleton<Scraper>();
                    services.AddSingleton<ApplicationCore.ApplicationCore>();
                });


        // private static ServiceCollection ConfigureServices()
        // {
        //     var serviceCollection = new ServiceCollection();
        //
        //     var config = new Config(LoadOrCreateConfigs(), Array.Empty<string>());
        //
        //     string? repositoryPath =
        //         Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");
        //
        //     ConfigureClients(serviceCollection, config);
        //    
        //     
        //     //need to add dict.
        //     var roundDataReaderService = new RoundDataReaderService(new Dictionary<uint256, RoundDataReaderService.ProcessedRound>());
        //     var roundDataProcessor = new RoundDataProcessor(roundDataReaderService);
        //     var roundDataFilter = new RoundsDataFilter(roundDataProcessor, roundDataReaderService);
        //     var analyzer = new Analyzer(roundDataReaderService, roundDataFilter);
        //     var betterHumanMonitor = new BetterHumanMonitor(roundDataFilter, roundDataProcessor, analyzer, roundDataReaderService);
        //
        //     serviceCollection.AddSingleton<RoundDataReaderService>(roundDataReaderService);
        //     serviceCollection.AddSingleton<RoundDataProcessor>(roundDataProcessor);
        //     serviceCollection.AddSingleton<IRoundsDataFilter>(roundDataFilter);
        //     serviceCollection.AddSingleton<IAnalyzer, Analyzer>();
        //     serviceCollection.AddSingleton<BetterHumanMonitor>();
        //
        //
        //
        //     serviceCollection.AddSingleton<FileProcessedRoundRepository>(serviceProvider =>
        //         new FileProcessedRoundRepository(repositoryPath));
        //     
        //     var serviceProvider = serviceCollection.BuildServiceProvider();
        //     
        //     serviceCollection.AddSingleton(new JsonRpcServerConfiguration(true, config!.JsonRpcUser,
        //         config.JsonRpcPassword, config.JsonRpcServerPrefixes));
        //
        //     serviceCollection.AddSingleton<JsonRpcServer>(serviceProvider =>
        //     {
        //         var jsonRpcServerConfig = serviceProvider.GetRequiredService<JsonRpcServerConfiguration>();
        //         var jsonRpcServer = new JsonRpcServer(
        //             new WabiSabiMonitorRpc(roundDataFilter, analyzer, betterHumanMonitor),
        //             jsonRpcServerConfig,
        //             new TerminateService(TerminateApplicationAsync, () => { })
        //         );
        //         return jsonRpcServer;
        //     });
        //
        //     var jsonRpcServer = serviceProvider.GetRequiredService<JsonRpcServer>();
        //     var jsonRpcServerConfiguration = serviceProvider.GetRequiredService<JsonRpcServerConfiguration>();
        //
        //     var rpcServerController = new RpcServerController(jsonRpcServer, jsonRpcServerConfiguration);
        //
        //     serviceCollection.AddSingleton<Scraper>();
        //     serviceCollection.AddSingleton<RpcServerController>(rpcServerController);
        //
        //     serviceCollection.BuildServiceProvider();
        //
        //     return serviceCollection;
        // }

        private static void ConfigureClients(ServiceCollection services, Config config)
        {
            services.AddHttpClient();

            services.AddSingleton<IHttpClient, ClearnetHttpClient>();

            var serviceProvider = services.BuildServiceProvider();

            var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var client = clientFactory.CreateClient();

            client.BaseAddress = config!.GetCoordinatorUri();

            services.AddSingleton<IWabiSabiApiRequestHandler>(
                new WabiSabiHttpApiClient(new ClearnetHttpClient(client)));
        }

        private static Task TerminateApplicationAsync()
        {
            Logger.LogInfo("Closing.");

            var repository = Di.ServiceProvider.GetRequiredService<FileProcessedRoundRepository>();

            repository?.SaveToFileSystem();

            var rpcServer = Di.ServiceProvider.GetRequiredService<JsonRpcServer>();
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
}