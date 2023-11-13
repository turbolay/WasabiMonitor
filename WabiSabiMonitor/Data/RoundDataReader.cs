using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data;

// it should be renamed to DataProcessor
public class RoundDataProcessor
{
    private RoundDataReaderService _roundDataReader;

    public RoundDataProcessor(RoundDataReaderService roundDataReader)
    {
        _roundDataReader = roundDataReader;
    }

    public List<RoundState> GetRounds(Func<RoundState, bool>? roundStatePredicate = null,
        Func<RoundDataReaderService.ProcessedRound, bool>? processedRoundPredicate = null)
    {
        return _roundDataReader!.Rounds.Values.ToList()
            .Where(x => processedRoundPredicate?.Invoke(x) ?? true)
            .Select(x => x.Round)
            .Where(x => roundStatePredicate?.Invoke(x) ?? true)
            .ToList();
    }

    public CoinJoinFeeRateMedian[] GetCurrentFeesConditions()
    {
        return _roundDataReader!.Rounds.Last().Value.CoinJoinFeeRateMedian;
    }
}