using NBitcoin;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data.Interfaces;

public interface IAnalyzer
{
    Analyzer.Analysis? AnalyzeRoundStates(List<RoundState> roundStates);
}