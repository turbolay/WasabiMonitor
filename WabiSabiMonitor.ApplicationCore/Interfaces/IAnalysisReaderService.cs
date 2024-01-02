namespace WabiSabiMonitor.ApplicationCore.Interfaces;

public interface IAnalysisReaderService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}