using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Adapters;

public interface IWabiSabiApiRequestHandlerAdapter
{
    Task<HumanMonitorResponse> GetHumanMonitor(HumanMonitorRequest request, CancellationToken cancellationToken);
}
