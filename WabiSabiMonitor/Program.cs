using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WabiSabiMonitor.Data;
using WabiSabiMonitor.Data.Interfaces;
using WabiSabiMonitor.Db;
using WabiSabiMonitor.Rpc;
using WabiSabiMonitor.Utils.Config;
using WabiSabiMonitor.Utils.Helpers;
using WabiSabiMonitor.Utils.Logging;
using WabiSabiMonitor.Utils.Rpc;
using WabiSabiMonitor.Utils.Services.Terminate;
using WabiSabiMonitor.Utils.Tor.Http;
using WabiSabiMonitor.Utils.WabiSabi.Backend.PostRequests;
using WabiSabiMonitor.Utils.WabiSabi.Client;

// ReSharper disable InconsistentlySynchronizedField

namespace WabiSabiMonitor
{
    public static partial class Program
    {
        public static Config? Config { get; set; }
        public static JsonRpcServer? RpcServer { get; private set; }
        public static RoundDataReaderService? DataProcessor { get; private set; }
        public static IHttpClientFactory? HttpClientFactory { get; set; }

        private static object LockClosure { get; } = new();

        public static WabiSabiHttpApiClient WabiSabiApiClient { get; private set; }

        private static readonly CancellationTokenSource CancellationTokenSource = new();

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

        private static void ConfigureClients()
        {
            Logger.LogInfo("creating http clients...");
            var clientFactory = Services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = clientFactory.CreateClient();

            client.BaseAddress = Config!.GetCoordinatorUri();

            Services.AddSingleton<IHttpClient, ClearnetHttpClient>();

            Services.AddSingleton<IWabiSabiApiRequestHandler>(
                new WabiSabiHttpApiClient(new ClearnetHttpClient(client)));

            // var httpClient = new HttpClient();
            // httpClient.BaseAddress = Config!.GetCoordinatorUri();
            // WabiSabiApiClient = new WabiSabiHttpApiClient();
        }

        public static async Task Main(string[] args)
        {
            var repositoryPath = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");

            try
            {
                var host = CreateHostBuilder(args).Build();
                
                ConfigureServices(host);
                
                await Start(host, repositoryPath);
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

        private static async Task Start(IHost host, string repositoryPath)
        {
            var roundStatusScraper = host.Services.GetRequiredService<Scraper>();
            var dataProcessor = host.Services.GetRequiredService<RoundDataReaderService>();
            var rpcServerController = host.Services.GetRequiredService<RpcServerController>();
            var repository = new FileProcessedRoundRepository(repositoryPath, dataProcessor);

            await host.RunAsync();

            Logger.InitializeDefaults("./logs.txt");
            Logger.LogInfo("Read confi...");

            await roundStatusScraper.StartAsync(CancellationTokenSource.Token);
            await roundStatusScraper.TriggerAndWaitRoundAsync(CancellationTokenSource.Token);

            await Scraper.ToBeProcessedData.Reader.WaitToReadAsync(CancellationTokenSource.Token);

            var DataProcessor = host.Services.GetRequiredService<RoundDataReaderService>();
            DataProcessor = new(repository.ReadFromFileSystem());
            await DataProcessor.StartAsync(CancellationTokenSource.Token);
            
            await rpcServerController.StartRpcServerAsync(CancellationTokenSource.Token);
            Logger.LogInfo("Initialized.");
            await Task.Delay(-1, CancellationTokenSource.Token);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args);

        private static void ConfigureServices(IHost host)
        {
            Logger.LogInfo("Configure services...");
            HandleClosure();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();

            ConfigureClients();

            Config = new Config(LoadOrCreateConfigs(), Array.Empty<string>());

            serviceCollection.AddSingleton(new JsonRpcServerConfiguration(true, Config!.JsonRpcUser,
                Config.JsonRpcPassword, Config.JsonRpcServerPrefixes));
            serviceCollection.AddSingleton<JsonRpcServer>(serviceProvider =>
            {
                var jsonRpcServerConfig = serviceProvider.GetRequiredService<JsonRpcServerConfiguration>();
                var jsonRpcServer = new JsonRpcServer(
                    new WabiSabiMonitorRpc(),
                    jsonRpcServerConfig,
                    new TerminateService(TerminateApplicationAsync, () => { })
                );
                return jsonRpcServer;
            });

            serviceCollection.AddSingleton<Scraper>();
            serviceCollection.AddSingleton<RoundDataReaderService>();
            serviceCollection.AddSingleton<IAnalyzer, Analyzer>();
            serviceCollection.AddSingleton<RpcServerController>();
            
            serviceCollection.BuildServiceProvider();
        }

        private static Task TerminateApplicationAsync()
        {
            Logger.LogInfo("Closing.");

            repository?.SaveToFileSystem();

            RpcServer?.Dispose();

            return Task.CompletedTask;
        }
    }
}