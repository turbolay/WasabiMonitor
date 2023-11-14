using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Interfaces;

public interface IRoundsDataFilter
{
    List<RoundState> GetRoundsInInterval(DateTimeOffset? start, DateTimeOffset? end,
        Func<RoundState, bool>? predicate = null);

    List<RoundState> GetCurrentRounds();

    List<RoundState> GetRoundsStartedSince(DateTimeOffset since, Func<RoundState, bool>? predicate = null);

    List<RoundState> GetRoundsStartedSince(TimeSpan since, Func<RoundState, bool>? predicate = null);

    List<RoundState> GetRoundsFinishedSince(DateTimeOffset since, Func<RoundState, bool>? predicate = null);

    List<uint256> GetBlameOf(RoundState roundState);
}