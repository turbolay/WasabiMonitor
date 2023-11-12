using System.Net;
using WabiSabiMonitor.Rpc;
using WabiSabiMonitor.Utils.Config;
using WabiSabiMonitor.Utils.Logging;
using WabiSabiMonitor.Utils.Rpc;
using WabiSabiMonitor.Utils.Services.Terminate;

namespace WabiSabiMonitor;

public class RpcServerService
{
    private static async Task StartRpcServerAsync(CancellationToken cancel, JsonRpcServer RpcServer)
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
}