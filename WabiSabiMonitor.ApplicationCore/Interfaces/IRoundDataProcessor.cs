using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Interfaces;

public interface IRoundDataProcessor
{
    List<RoundState> GetRounds(
        Func<RoundState, bool>? roundStatePredicate = null, 
        Func<RoundDataReaderService.ProcessedRound, bool>? processedRoundPredicate = null);

    CoinJoinFeeRateMedian[] GetCurrentFeesConditions();
}