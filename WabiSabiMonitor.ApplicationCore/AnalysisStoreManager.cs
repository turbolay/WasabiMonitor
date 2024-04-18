using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Bases;
using WabiSabiMonitor.ApplicationCore.Utils.Helpers;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;
using static WabiSabiMonitor.ApplicationCore.Data.Analyzer;

namespace WabiSabiMonitor.ApplicationCore
{
    public class AnalysisStoreManager : PeriodicRunner
    {
        private static readonly string _directoryPath = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "DataStore", "Analysis")));
        private readonly IAnalyzer _analyzer;
        private readonly IRoundsDataFilter _roundsDataFilter;
        private DateTime _lastDate;
        private Dictionary<DateTime, Analysis> Analysis { get; }

        public AnalysisStoreManager(IAnalyzer analyzer, IRoundsDataFilter roundsDataFilter, TimeSpan period) : base(period)
        {
            _analyzer = analyzer;
            _roundsDataFilter = roundsDataFilter;
            _lastDate = DateTime.UtcNow;
            Analysis = LoadFromFile();
        }

        protected override async Task ActionAsync(CancellationToken cancel)
        {
            var now = DateTime.UtcNow;
            DateTime startTime = now.Subtract(TimeSpan.FromHours(24));

            // Analysis over a rolling 24h.
            var roundStates = _roundsDataFilter.GetRoundsFinishedSince(startTime);
            var analysis = _analyzer.AnalyzeRoundStates(roundStates);
            if (analysis is null)
            {
                return;
            }
            Analysis.Add(now, analysis);

            if (now.Date != _lastDate.Date)
            {
                IoHelpers.EnsureDirectoryExists(_directoryPath);

                // Save the last 24 hours data to file at the end of the day.
                var path = Path.Combine(_directoryPath, $"Analysis_{_lastDate:yyyy-MM-dd}.json");

                // Compute a new Analysis object for these ones to have exactly the adequate rounds.
                var lastDayRounds = _roundsDataFilter.GetRoundsFinishedInInterval(startTime, now.Date);

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

        private static Dictionary<DateTime, Analysis> LoadFromFile()
        {
            if (!Directory.Exists(_directoryPath))
            {
                return new();
            }

            var analyses = new Dictionary<DateTime, Analysis>();
            var dataFiles = Directory.EnumerateFiles(_directoryPath);

            // Get Analysis from the past 15 days. Start with 1 as there supposed to be no daily analisys yet for UtcNow.
            for (int days = 1; days <= 15; days++)
            {
                var date = DateTime.UtcNow.Subtract(TimeSpan.FromDays(days));
                var dateString = date.ToString("yyyy-MM-dd");
                var file = dataFiles.SingleOrDefault(filename => filename.Contains(dateString));
                if (file == null) continue;
                var data = ReadFromFile(file);
                if (data == null) continue;
                analyses.Add(date, data);
            }
            return analyses;
        }

        public static Analysis? ReadFromFile(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Analysis>(json);
            }
            catch (Exception exc)
            {
                Logger.LogError($"Error while reading Analysis from file {path}.", exc);
            }
            return null;
        }
    }
}