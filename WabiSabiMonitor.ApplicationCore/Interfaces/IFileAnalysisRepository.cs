using WabiSabiMonitor.ApplicationCore.Data;

namespace WabiSabiMonitor.ApplicationCore.Interfaces;

public interface IFileAnalysisRepository
{
    void SaveToFileSystem(Analyzer.Analysis data);
    Analyzer.Analysis? ReadFromFileSystem();
}