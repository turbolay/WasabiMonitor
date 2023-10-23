using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.Data;
using WabiSabiMonitor.Utils.Helpers;
using WabiSabiMonitor.Utils.WabiSabi.Models.Serialization;

namespace WabiSabiMonitor.Db;

public static class Db
{
    private static string DbPath { get; } = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), "data.json");

    public static void SaveToFileSystem()
    {
        File.WriteAllText(DbPath, JsonConvert.SerializeObject(Program.DataProcessor!.Rounds, JsonSerializationOptions.CurrentSettings));
    }
    
    public static Dictionary<uint256, Processor.ProcessedRound>? ReadFromFileSystem()
    {
        try
        {
            return JsonConvert.DeserializeObject<Dictionary<uint256, Processor.ProcessedRound>>(
                File.ReadAllText(DbPath), JsonSerializationOptions.CurrentSettings);
        }
        catch(Exception ex)
        {
            return null;
        }
    }
}