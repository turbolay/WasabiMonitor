using NBitcoin;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data.Interfaces;

public interface IAnalyzer
{
    List<RoundState> GetCurrentRounds();
    
    List<RoundState> GetRoundsStartedSince(DateTimeOffset since, Func<RoundState, bool>? predicate = null);

    List<RoundState> GetRoundsStartedSince(TimeSpan since, Func<RoundState, bool>? predicate = null);

    List<RoundState> GetRoundsFinishedSince(DateTimeOffset since, Func<RoundState, bool>? predicate = null);

    List<RoundState> GetRoundsInInterval(DateTimeOffset? start, DateTimeOffset? end,
        Func<RoundState, bool>? predicate = null);
    
    List<uint256> GetBlameOf(RoundState roundState);

    Analyzer.Analysis? AnalyzeRoundStates(List<RoundState> roundStates);
}