using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data.Interfaces;

public interface IWabiSabiApiRequestHandlerAdapter
{
    Task<HumanMonitorResponse> GetHumanMonitor(HumanMonitorRequest request, CancellationToken cancellationToken);
}
