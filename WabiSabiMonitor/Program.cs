using System.Net;
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
using WabiSabiMonitor.Utils.WabiSabi.Client;
using WabiSabiMonitor.Utils.WabiSabi.Models;
// ReSharper disable InconsistentlySynchronizedField

namespace WabiSabiMonitor
{
    public static class Program
    {
        public static Config? Config { get; set; }
        public static JsonRpcServer? RpcServer { get; private set; }
        public static Processor? DataProcessor { get; private set; }
        public static IHttpClientFactory? HttpClientFactory {get; set;}
        public static IServiceCollection Services {get; set;}

        private static object LockClosure { get; } = new();
        
        private static readonly CancellationTokenSource CancellationTokenSource = new ();

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
    
        private static async Task StartRpcServerAsync(CancellationToken cancel)
        {
            var jsonRpcServerConfig = new JsonRpcServerConfiguration(true, Config!.JsonRpcUser, Config.JsonRpcPassword, Config.JsonRpcServerPrefixes);
            if (jsonRpcServerConfig.IsEnabled)
            {
                var jsonRpcService = new WabiSabiMonitorRpc();
                RpcServer = new JsonRpcServer(jsonRpcService, jsonRpcServerConfig, new TerminateService(TerminateApplicationAsync, () => { }));
                try
                {
                    await RpcServer.StartAsync(cancel).ConfigureAwait(false);
                }
                catch (HttpListenerException e)
                {
                    Logger.LogWarning($"Failed to start {nameof(JsonRpcServer)} with error: {e.Message}.");
                    RpcServer = null;
                }
            }
        }
        
        
        
        private static PersistentConfig LoadOrCreateConfigs()
        {
            Directory.CreateDirectory(Config.DataDir);

            PersistentConfig persistentConfig = new(Path.Combine(Config.DataDir, "Config.json"));
            persistentConfig.LoadFile(createIfMissing: true);

            return persistentConfig;
        }

        private static void CreateHttpClients()
        {
            Services = ConfigureServices();

            var clientFactory = Services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            HttpClientFactory = clientFactory;
            var httpClient = HttpClientFactory.CreateClient();
    
            httpClient.BaseAddress = Config!.GetCoordinatorUri();
            WabiSabiApiClient = new WabiSabiHttpApiClient(new ClearnetHttpClient(httpClient));
            
            // var httpClient = new HttpClient();
            // httpClient.BaseAddress = Config!.GetCoordinatorUri();
            // WabiSabiApiClient = new WabiSabiHttpApiClient(new ClearnetHttpClient(httpClient));
        }
        
        private static IServiceCollection ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            return serviceCollection;
        }
        
        public static async Task Main(string[] args)
        {
            //moved here
            var host = CreateHostBuilder(args).Build();
            var repositoryPath = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");
            
            try
            {
                HandleClosure();
                var WabiSabiHttpApiClient = host.Services.GetRequiredService<WabiSabiHttpApiClient>();
                var roundStatusScraper = host.Services.GetRequiredService<Scraper>();   
                var dataProcessor = host.Services.GetRequiredService<Processor>();

                var repository = new FileProcessedRoundRepository(repositoryPath, dataProcessor);

                await host.RunAsync();
                
                Logger.InitializeDefaults("./logs.txt");
                Logger.LogInfo("Read confi...");
                Config = new Config(LoadOrCreateConfigs(), Array.Empty<string>());
                Logger.LogInfo("creating http clients...");
                CreateHttpClients();
                
                await roundStatusScraper.StartAsync(CancellationTokenSource.Token);
                await roundStatusScraper.TriggerAndWaitRoundAsync(CancellationTokenSource.Token);

                await Scraper.ToBeProcessedData.Reader.WaitToReadAsync(CancellationTokenSource.Token);

                var DataProcessor = host.Services.GetRequiredService<Processor>(); 
                DataProcessor = new(repository.ReadFromFileSystem());
                await DataProcessor.StartAsync(CancellationTokenSource.Token);
                
                await StartRpcServerAsync(CancellationTokenSource.Token);
                Logger.LogInfo("Initialized.");
                await Task.Delay(-1, CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
            }
            finally
            {
                await TerminateApplicationAsync(repository);
            }
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<WabiSabiHttpApiClient>();
                    services.AddSingleton<Scraper>();
                    services.AddSingleton<Processor>();
                    services.AddSingleton<Analyzer>();
                });

        private static void TerminateApplicationAsync(IProcessedRoundRepository repository)
        {
            Logger.LogInfo("Closing.");

            repository.SaveToFileSystem();
            RpcServer?.Dispose();
        }

        public record PublicStatus(DateTimeOffset ScrapedAt, RoundStateResponse Rounds, HumanMonitorResponse HumanMonitor);
    }
}