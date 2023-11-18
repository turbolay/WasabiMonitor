using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Interfaces;

public interface IWabiSabiApiRequestHandlerAdapter
{
    Task<HumanMonitorResponse> GetHumanMonitor(HumanMonitorRequest request, CancellationToken cancellationToken);
}
