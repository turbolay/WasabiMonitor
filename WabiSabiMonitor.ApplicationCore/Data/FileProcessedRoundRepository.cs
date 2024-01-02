using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class FileProcessedRoundRepository : IProcessedRoundRepository
{
    private readonly string _path;

    //  path = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");
    public FileProcessedRoundRepository(string path)
    {
        _path = path;
    }

    public void SaveToFileSystem(Dictionary<uint256, RoundDataReaderService.ProcessedRound> data)
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

    public Dictionary<uint256, RoundDataReaderService.ProcessedRound>? ReadFromFileSystem()
    {
        try
        {
            return JsonConvert.DeserializeObject<Dictionary<uint256, RoundDataReaderService.ProcessedRound>>(
                File.ReadAllText(_path), JsonSerializationOptions.CurrentSettings);
        }
        catch (Exception ex)
        {
            return new Dictionary<uint256, RoundDataReaderService.ProcessedRound>();
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

        File.Copy(_path, backupFilePath, true);

        return backupFilePath;
    }
}