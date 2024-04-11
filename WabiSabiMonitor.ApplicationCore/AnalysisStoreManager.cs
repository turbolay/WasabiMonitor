using Microsoft.Extensions.Hosting;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Bases;

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
            while (!cancel.IsCancellationRequested)
            {
                var date = DateTime.Now.Date;
                var roundStates = _roundsDataFilter.GetRoundsStartedSince(date);
                var analysis = _analyzer.AnalyzeRoundStates(roundStates);
            }
        }
    }
}