using Newtonsoft.Json;
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

        public AnalysisStoreManager(IAnalyzer analyzer, IRoundsDataFilter roundsDataFilter, TimeSpan period) : base(period)
        {
            _analyzer = analyzer;
            _roundsDataFilter = roundsDataFilter;
        }

        protected override async Task ActionAsync(CancellationToken cancel)
        {
            var date = DateTime.Now.Date;
            var path = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "DataStore", "Analysis", "Client")), $"Analysis_{date:yyyy-MM-dd}.json");

            var roundStates = _roundsDataFilter.GetRoundsStartedSince(date);
            var analysis = _analyzer.AnalyzeRoundStates(roundStates);

            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(analysis, JsonSerializationOptions.CurrentSettings), cancel);
        }
    }
}