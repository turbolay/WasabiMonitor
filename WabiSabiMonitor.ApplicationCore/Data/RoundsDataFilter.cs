using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class RoundsDataFilter : IRoundsDataFilter
{
    private readonly IRoundDataProcessor _roundDataProcessor;
    private readonly IRoundDataReaderService _roundDataReaderService;

    public RoundsDataFilter( IRoundDataProcessor roundDataProcessor, IRoundDataReaderService roundDataReaderService)
    {
        _roundDataProcessor = roundDataProcessor;
        _roundDataReaderService = roundDataReaderService;
    }

    public List<RoundState> GetRoundsFinishedInInterval(DateTimeOffset? start, DateTimeOffset? end,
        Func<RoundState, bool>? predicate = null) =>
        _roundDataProcessor.GetRounds(
                x => !x.IsOngoing() && (predicate?.Invoke(x) ?? true),
            x => (start is null || x.LastUpdate >= start) && (end is null || x.LastUpdate < end));

    // The rounds with EndRoundState.None are filtered to only keep the ones that started less than 2h ago or since initialization.
    // This is to avoid false positives when we quit the app for long time.
    // A full fix would be to discard those rounds from the database.
    public List<RoundState> GetCurrentRounds() =>
        _roundDataProcessor.GetRounds(x => (x.InputRegistrationStart > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(2)) ||
                                           x.InputRegistrationStart > ApplicationCore.LastInit)
                                           && x.EndRoundState == EndRoundState.None);

    public List<RoundState> GetRoundsStartedSince(DateTimeOffset since, Func<RoundState, bool>? predicate = null) =>
        _roundDataProcessor.GetRounds(x => x.InputRegistrationStart >= since && (predicate?.Invoke(x) ?? true));

    public List<RoundState> GetRoundsStartedSince(TimeSpan since, Func<RoundState, bool>? predicate = null) =>
        GetRoundsStartedSince(DateTimeOffset.UtcNow - since, predicate);

    public List<RoundState> GetRoundsFinishedSince(DateTimeOffset since, Func<RoundState, bool>? predicate = null) =>
        _roundDataProcessor.GetRounds(
            x => !x.IsOngoing() && (predicate?.Invoke(x) ?? true),
            x => x.LastUpdate >= since);

    public uint GetNbBanEstimation( RoundState roundState)
    {
        if (!roundState.IsBlame())
        {
            return 0;
        }

        var blameOf = GetRoundsStartedSince(TimeSpan.FromHours(1))
            .FirstOrDefault(x => x.BlameOf == roundState.Id);
        
        if (blameOf is null)
        {
            return 0;
        }

        // possible minus
        return roundState.GetConfirmedInputsCount() - blameOf.GetInputsCount(_roundDataReaderService);
    }
    public List<uint256> GetBlameOf(RoundState roundState)
    {
        if (roundState.BlameOf == uint256.Zero)
        {
            return new List<uint256>();
        }

        var result = new List<uint256>();
        var toSearchId = roundState.BlameOf;

        while (true)
        {
            var blameOf = GetRoundsStartedSince(TimeSpan.FromHours(1)).FirstOrDefault(x => x.Id == toSearchId);
            if (blameOf is null)
            {
                result.Add(roundState.BlameOf);
                break;
            }

            result.Add(toSearchId);
            toSearchId = blameOf.BlameOf;
        }

        return result;
    }
}