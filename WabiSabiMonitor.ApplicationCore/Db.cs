using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;

namespace WabiSabiMonitor.ApplicationCore;

public class FileProcessedRoundRepository : IProcessedRoundRepository
{
    private readonly string _path;
    private RoundDataReaderService _dataProcessor;

    //  path = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");
    public FileProcessedRoundRepository(string path, RoundDataReaderService dataProcessor)
    {
        _path = path;
        _dataProcessor = dataProcessor;
    }

    //?
    public void SaveToFileSystem()
    {
        File.WriteAllText(_path,
            JsonConvert.SerializeObject(_dataProcessor!.Rounds, JsonSerializationOptions.CurrentSettings));
    }

    public Dictionary<uint256, Processor.ProcessedRound>? ReadFromFileSystem()
    {
        try
        {
            return JsonConvert.DeserializeObject<Dictionary<uint256, Processor.ProcessedRound>>(
                File.ReadAllText(_path), JsonSerializationOptions.CurrentSettings);
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}