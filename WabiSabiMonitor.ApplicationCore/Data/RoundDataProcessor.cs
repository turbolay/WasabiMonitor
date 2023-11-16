using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class RoundDataProcessor : IRoundDataProcessor
{
    private readonly RoundDataReaderService _roundDataReader;

    public RoundDataProcessor(RoundDataReaderService roundDataReaderService)
    {
        _roundDataReader = roundDataReaderService;
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