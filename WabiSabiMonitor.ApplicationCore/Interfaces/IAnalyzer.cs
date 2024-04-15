using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Interfaces;

public interface IAnalyzer
{
    Analyzer.Analysis? AnalyzeRoundStates(List<RoundState> roundStates);
}