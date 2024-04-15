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
        public Dictionary<DateTime, Analyzer.Analysis> Analysis { get; private set; }

        public AnalysisStoreManager(IAnalyzer analyzer, IRoundsDataFilter roundsDataFilter, TimeSpan period) : base(period)
        {
            _analyzer = analyzer;
            _roundsDataFilter = roundsDataFilter;
            _lastDate = DateTime.Now;
            Analysis = new();
        }

        protected override async Task ActionAsync(CancellationToken cancel)
        {
            var now = DateTime.Now;
            TimeSpan startTime = TimeSpan.FromHours(24);

            var roundStates = _roundsDataFilter.GetRoundsStartedSince(startTime);
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
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(analysis, JsonSerializationOptions.CurrentSettings), cancel);

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