using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Adapters;

public class WabiSabiApiRequestHandlerAdapter : IWabiSabiApiRequestHandlerAdapter
{
    private readonly WabiSabiHttpApiClient _client;

    public WabiSabiApiRequestHandlerAdapter(WabiSabiHttpApiClient client)
    {
        _client = client;
    }

    public Task<HumanMonitorResponse> GetHumanMonitor(HumanMonitorRequest request, CancellationToken cancellationToken)
    {
        // google "when I shouldn't use await in Task-return methods"
        return _client.GetHumanMonitor(request, cancellationToken);    
    }
}