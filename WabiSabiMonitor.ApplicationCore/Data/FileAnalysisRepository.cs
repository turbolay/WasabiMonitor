using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class FileAnalysisRepository : IFileAnalysisRepository
{
    private readonly string _path;

    public FileAnalysisRepository(string path)
    {
        _path = path;
    }

    public void SaveToFileSystem(Analyzer.Analysis data)
    {
        var backupPath = CreateBackup();

        try
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(data, JsonSerializationOptions.CurrentSettings));
            File.Delete(backupPath);
        }
        catch (Exception e)
        {
            RestoreFromBackup(backupPath);
            throw;
        }
    }

    public Analyzer.Analysis? ReadFromFileSystem()
    {
        try
        {
            return JsonConvert.DeserializeObject<Analyzer.Analysis>(
                File.ReadAllText(_path), JsonSerializationOptions.CurrentSettings);
        }
        catch (Exception ex)
        {
            return default(Analyzer.Analysis);
        }
    }

    private void RestoreFromBackup(string backupPath)
    {
        File.Copy(backupPath, _path, true);
        File.Delete(backupPath);
    }

    private string CreateBackup()
    {
        var backupFileName = Path.GetFileNameWithoutExtension(_path);
        var backupFileExtension = Path.GetExtension(_path);
        var backupFolderName = Path.GetDirectoryName(_path);
        var backupFilePath = backupFolderName + "\\" + backupFileName + "_backup" + backupFileExtension;

        if (File.Exists(_path))
        {
            File.Copy(_path, backupFilePath, true);
        }

        return backupFilePath;
    }
}