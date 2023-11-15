using System.Net;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.Rpc;

namespace WabiSabiMonitor.ApplicationCore;

public class RpcServerController
{
    private readonly JsonRpcServer _rpcServer;
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