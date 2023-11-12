using System.Diagnostics;
using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.Data;
using WabiSabiMonitor.Data.Interfaces;
using WabiSabiMonitor.Utils.Helpers;
using WabiSabiMonitor.Utils.WabiSabi.Models.Serialization;

namespace WabiSabiMonitor.Db;

public class FileProcessedRoundRepository : IProcessedRoundRepository
{
    private readonly string _path;
    private Processor _dataProcessor;
    
    //  private static string DbPath { get; } = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");
    public FileProcessedRoundRepository(string path, Processor dataProcessor)
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