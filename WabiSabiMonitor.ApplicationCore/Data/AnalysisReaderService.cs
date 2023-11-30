using Microsoft.Extensions.Hosting;
using WabiSabiMonitor.ApplicationCore.Interfaces;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class AnalysisReaderService : BackgroundService, IAnalysisReaderService
{
    private readonly IRoundsDataFilter _filter;
    private readonly IAnalyzer _analyzer;
    private readonly TimeSpan _interval;
    private readonly IFileAnalysisRepository _repository;

    public AnalysisReaderService(IRoundsDataFilter filter, IAnalyzer analyzer, TimeSpan interval,
        IFileAnalysisRepository repository)
    {
        _filter = filter;
        _analyzer = analyzer;
        _interval = interval;
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);

        do
        {
            var startTime = DateTime.UtcNow - TimeSpan.FromHours(24);
            var result = _analyzer.AnalyzeRoundStates(_filter.GetRoundsInInterval(startTime, DateTime.UtcNow));

            if (result is not null)
            {
                _repository.SaveToFileSystem(result);
            }
        } while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken));
    }
}