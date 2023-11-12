using WabiSabiMonitor.Data;

namespace WabiSabiMonitor.Rpc.Models;

public record BetterHumanMonitorModel(List<BetterHumanMonitor.BetterHumanMonitorRound> CurrentRounds,
    List<BetterHumanMonitor.BetterHumanMonitorRound> LastPeriod)
{
    public static BetterHumanMonitorModel Empty() => new(new(), new());
    public Analyzer.Analysis? Analysis { get; set; }
}