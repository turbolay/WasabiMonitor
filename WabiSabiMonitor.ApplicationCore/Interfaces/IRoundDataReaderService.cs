using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Interfaces;

public interface IRoundDataReaderService
{
    Dictionary<uint256, RoundDataReaderService.ProcessedRound> Rounds { get; }
    HumanMonitorResponse? LastHumanMonitor { get; }
    Task? ExecuteTask { get; }
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    void Dispose();
}