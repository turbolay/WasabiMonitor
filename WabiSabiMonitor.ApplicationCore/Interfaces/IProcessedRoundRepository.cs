using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Data;

namespace WabiSabiMonitor.ApplicationCore.Interfaces;

public interface IProcessedRoundRepository
{
    void SaveToFileSystem();
    Dictionary<uint256, Processor.ProcessedRound>? ReadFromFileSystem();
}