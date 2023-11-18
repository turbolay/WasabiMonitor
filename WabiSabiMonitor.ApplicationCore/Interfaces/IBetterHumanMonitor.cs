using WabiSabiMonitor.ApplicationCore.Rpc.Models;

namespace WabiSabiMonitor.ApplicationCore.Interfaces;

public interface IBetterHumanMonitor
{
    BetterHumanMonitorModel GetApiResponse(DateTimeOffset? start = null, DateTimeOffset? end = null);
}