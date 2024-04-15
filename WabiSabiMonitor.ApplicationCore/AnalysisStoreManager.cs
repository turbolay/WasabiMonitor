using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Bases;
using WabiSabiMonitor.ApplicationCore.Utils.Helpers;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;

namespace WabiSabiMonitor.ApplicationCore
{
    public class AnalysisStoreManager : PeriodicRunner
    {
        private readonly IAnalyzer _analyzer;
        private readonly IRoundsDataFilter _roundsDataFilter;
        private DateTime _lastDate;
        private Dictionary<DateTime, Analyzer.Analysis> Analysis { get;} = new();

        public AnalysisStoreManager(IAnalyzer analyzer, IRoundsDataFilter roundsDataFilter, TimeSpan period) : base(period)
        {
            _analyzer = analyzer;
            _roundsDataFilter = roundsDataFilter;
            _lastDate = DateTime.UtcNow;
        }

        protected override async Task ActionAsync(CancellationToken cancel)
        {
            var now = DateTime.UtcNow;
            TimeSpan startTime = TimeSpan.FromHours(24);

            // Analysis over a rolling 24h.
            var roundStates = _roundsDataFilter.GetRoundsFinishedSince(now.Subtract(startTime));
            var analysis = _analyzer.AnalyzeRoundStates(roundStates);
            if (analysis is null)
            {
                return;
            }
            Analysis.Add(now, analysis);

            if (now.Date != _lastDate.Date)
            {
                // Save the last 24 hours data to file at the end of the day.
                var path = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "DataStore", "Analysis")), $"Analysis_{_lastDate:yyyy-MM-dd}.json");
                
                // Compute a new Analysis object for these ones to have exactly the adequate rounds.
                var lastDayRounds = _roundsDataFilter.GetRoundsFinishedInInterval(now.Subtract(startTime), now.Date);
               
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(_analyzer.AnalyzeRoundStates(lastDayRounds), JsonSerializationOptions.CurrentSettings), cancel);

                // Remove data older than 15 days.
                CleanupOldAnalysisData(now);
                _lastDate = now;
            }
        }

        private void CleanupOldAnalysisData(DateTime now)
        {
            var thresholdDate = now.Date.AddDays(-15);
            var dataToRemove = Analysis.Keys.Where(date => date < thresholdDate);

            foreach (var data in dataToRemove)
            {
                Analysis.Remove(data);
            }
        }
    }
}