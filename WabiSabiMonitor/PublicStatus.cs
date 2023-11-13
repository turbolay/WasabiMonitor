using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor
{
    public record PublicStatus(DateTimeOffset ScrapedAt, RoundStateResponse Rounds, HumanMonitorResponse HumanMonitor);
}