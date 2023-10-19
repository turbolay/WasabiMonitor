using System.Net;
using WabiSabiMonitor.ApiClient;
using WabiSabiMonitor.Utils.Config;
using WabiSabiMonitor.Utils.Logging;
using WabiSabiMonitor.Utils.Rpc;

namespace WabiSabiMonitor
{
    public static class Program
    {
        public static Config? Config { get; set; }
        public static JsonRpcServer? RpcServer { get; private set; }
        
        private static readonly CancellationTokenSource CancellationTokenSource = new ();

        private static void HandleClosure()
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent the default Ctrl+C behavior
                CancellationTokenSource.Cancel();
            };   
        }
    
        private static async Task StartRpcServerAsync(CancellationToken cancel)
        {
            var jsonRpcServerConfig = new JsonRpcServerConfiguration(true, Config!.JsonRpcUser, Config.JsonRpcPassword, Config.JsonRpcServerPrefixes);
            if (jsonRpcServerConfig.IsEnabled)
            {
                var jsonRpcService = new WabiSabiMonitorApiClient();
                RpcServer = new JsonRpcServer(jsonRpcService, jsonRpcServerConfig);
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
        
        public static async Task Main(string[] args)
        {
            try
            {
                Console.Clear();
                HandleClosure();
                Logger.InitializeDefaults("./logs.txt");
                
                Config = new Config(LoadOrCreateConfigs(), Array.Empty<string>());
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
                Logger.LogInfo("Closing.");
                RpcServer?.Dispose();
            }
        }
    }
}