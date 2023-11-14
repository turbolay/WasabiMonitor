using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using WabiSabiMonitor.Db;

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

                Di.ServiceProvider = ConfigureServices()
                    .BuildServiceProvider();

                await new ApplicationCore.ApplicationCore().Run();
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

        private static ServiceCollection ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            var config = new Config(LoadOrCreateConfigs(), Array.Empty<string>());
            
            var repositoryPath = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");

            ConfigureClients(serviceCollection, config);

            var analyzer = new Analyzer();
            var roundDataFilter = new RoundsDataFilter();

            var roundDataReaderSevice = new RoundDataReaderService();

            var roundDataProcessor = new RoundDataProcessor(roundDataReaderSevice);

            var betterGumanMonitor = new BetterHumanMonitor(roundDataFilter, );

            var repository = new FileProcessedRoundRepository(repositoryPath, dataProcessor);

            serviceCollection.AddSingleton<IAnalyzer>(analyzer);
            serviceCollection.AddSingleton<IRoundsDataFilter>(roundDataFilter);

            serviceCollection.AddSingleton(new JsonRpcServerConfiguration(true, config!.JsonRpcUser,
                config.JsonRpcPassword, config.JsonRpcServerPrefixes));

            serviceCollection.AddSingleton<JsonRpcServer>(serviceProvider =>
            {
                var jsonRpcServerConfig = serviceProvider.GetRequiredService<JsonRpcServerConfiguration>();
                var jsonRpcServer = new JsonRpcServer(
                    new WabiSabiMonitorRpc(roundDataFilter, analyzer, betterGumanMonitor),
                    jsonRpcServerConfig,
                    new TerminateService(TerminateApplicationAsync, () => { })
                );
                return jsonRpcServer;
            });

            serviceCollection.AddSingleton<Scraper>();
            serviceCollection.AddSingleton<RoundDataReaderService>();
            serviceCollection.AddSingleton<RpcServerController>();

            serviceCollection.BuildServiceProvider();

            return serviceCollection;
        }

        private static void ConfigureClients(ServiceCollection services, Config config)
        {
            var clientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            var client = clientFactory.CreateClient();

            client.BaseAddress = config!.GetCoordinatorUri();

            services.AddSingleton<IHttpClient, ClearnetHttpClient>();

            services.AddSingleton<IWabiSabiApiRequestHandler>(
                new WabiSabiHttpApiClient(new ClearnetHttpClient(client)));
        }

        private static Task TerminateApplicationAsync()
        {
            Logger.LogInfo("Closing.");

            repository?.SaveToFileSystem();

            RpcServer?.Dispose();

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