using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data;

public class RoundDataReader
{
    private Processor _dataProcessor;

    public RoundDataReader(Processor dataProcessor)
    {
        _dataProcessor = dataProcessor;
    }

    public List<RoundState> GetRounds(Func<RoundState, bool>? roundStatePredicate = null,
        Func<Processor.ProcessedRound, bool>? processedRoundPredicate = null)
    {
        return _dataProcessor!.Rounds.Values.ToList()
            .Where(x => processedRoundPredicate?.Invoke(x) ?? true)
            .Select(x => x.Round)
            .Where(x => roundStatePredicate?.Invoke(x) ?? true)
            .ToList();
    }

    public CoinJoinFeeRateMedian[] GetCurrentFeesConditions()
    {
        return _dataProcessor!.Rounds.Last().Value.CoinJoinFeeRateMedian;
    }
}