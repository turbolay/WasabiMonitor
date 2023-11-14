using NBitcoin;
using WabiSabiMonitor.Data.Models;

namespace WabiSabiMonitor.Data.Interfaces;

public interface IProcessedRoundRepository
{
    void SaveToFileSystem();
    Dictionary<uint256, Processor.ProcessedRound>? ReadFromFileSystem();
}