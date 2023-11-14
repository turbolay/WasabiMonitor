using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore
{
    public record PublicStatus(DateTimeOffset ScrapedAt, RoundStateResponse Rounds, HumanMonitorResponse HumanMonitor);
}