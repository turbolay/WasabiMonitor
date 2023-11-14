using System.Net;
using WabiSabiMonitor.Data.Interfaces;
using WabiSabiMonitor.Rpc;
using WabiSabiMonitor.Utils.Config;
using WabiSabiMonitor.Utils.Logging;
using WabiSabiMonitor.Utils.Rpc;
using WabiSabiMonitor.Utils.Services.Terminate;

namespace WabiSabiMonitor;

public class RpcServerController
{
    private JsonRpcServer _rpcServer;
    private readonly JsonRpcServerConfiguration _jsonRpcServerConfiguration;

    public RpcServerController(JsonRpcServer rpcServer, JsonRpcServerConfiguration jsonRpcServerConfiguration)
    {
        _rpcServer = rpcServer;
        _jsonRpcServerConfiguration = jsonRpcServerConfiguration;
    }

    public async Task StartRpcServerAsync(CancellationToken cancel)
    {
        if (_jsonRpcServerConfiguration.IsEnabled)
        {
            try
            {
                await _rpcServer.StartAsync(cancel)
                    .ConfigureAwait(false);
            }
            catch (HttpListenerException e)
            {
                Logger.LogWarning($"Failed to start {nameof(JsonRpcServer)} with error: {e.Message}.");
            }
        }
    }
}