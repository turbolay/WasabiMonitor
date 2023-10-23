using System.Net;
using WabiSabiMonitor.Data;
using WabiSabiMonitor.Rpc;
using WabiSabiMonitor.Utils.Config;
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
        public static WabiSabiHttpApiClient? WabiSabiApiClient { get; private set; }
        public static Scraper? RoundStatusScraper { get; private set; }
        public static Processor? DataProcessor { get; private set; }

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
            var httpClient = new HttpClient();
            httpClient.BaseAddress = Config!.GetCoordinatorUri();
            WabiSabiApiClient = new WabiSabiHttpApiClient(new ClearnetHttpClient(httpClient));
        }
        
        public static async Task Main(string[] args)
        {
            try
            {
                HandleClosure();
                Logger.InitializeDefaults("./logs.txt");
                
                Config = new Config(LoadOrCreateConfigs(), Array.Empty<string>());
                CreateHttpClients();
                RoundStatusScraper = new();
                await RoundStatusScraper.StartAsync(CancellationTokenSource.Token);
                await RoundStatusScraper.TriggerAndWaitRoundAsync(CancellationTokenSource.Token);

                await Scraper.ToBeProcessedData.Reader.WaitToReadAsync(CancellationTokenSource.Token);
                
                DataProcessor = new(Db.Db.ReadFromFileSystem() ?? new());
                await DataProcessor.StartAsync(CancellationTokenSource.Token);
                
                await StartRpcServerAsync(CancellationTokenSource.Token);
                Logger.LogInfo("Initialized.");
                await Task.Delay(-1, CancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Normal with ctrl+c
            }
            finally
            {
                await TerminateApplicationAsync();
            }
        }

        private static Task TerminateApplicationAsync()
        {
            Logger.LogInfo("Closing.");
            Db.Db.SaveToFileSystem();
            RpcServer?.Dispose();
            return Task.CompletedTask;
        }

        public record PublicStatus(DateTimeOffset ScrapedAt, RoundStateResponse Rounds, HumanMonitorResponse HumanMonitor);
    }
}