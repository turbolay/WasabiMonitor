using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class FileProcessedRoundRepository : IProcessedRoundRepository
{
    private readonly string _path;

    public FileProcessedRoundRepository(string path)
    {
        _path = path;
    }

    public void SaveToFileSystem(Dictionary<uint256, RoundDataReaderService.ProcessedRound> data)
    {
        File.WriteAllText(_path, JsonConvert.SerializeObject(data, JsonSerializationOptions.CurrentSettings));
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
            return null;
        }
    }
}