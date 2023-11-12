using WabiSabiMonitor.Data;
using WabiSabiMonitor.Rpc.Models;
using WabiSabiMonitor.Utils.Logging;
using WabiSabiMonitor.Utils.Rpc;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Rpc;
public class WabiSabiMonitorRpc : IJsonRpcService
{
    [JsonRpcMethod("echo")]
    public string HelloWorld()
    {
        var message = "Hello world!";
        Logger.LogInfo(message);
        return message;
    }
	
    [JsonRpcMethod("better-human-monitor")]
    public BetterHumanMonitorModel GetBetterHumanMonitor() =>
        BetterHumanMonitor.GetApiResponse();
	
    [JsonRpcMethod("get-analysis")]
    public Analyzer.Analysis? GetAnalysis(string? startTime = null, string? endTime = null)
    {
        var (startDateTime, endDateTime) = ParseInterval(startTime, endTime);
        if (startTime is null && endTime is null)
        {
            startDateTime = DateTime.UtcNow - TimeSpan.FromHours(12);
        }
        return Analyzer.AnalyzeRoundStates(Analyzer.GetRoundsInInterval(startDateTime, endDateTime));
    }
    
    [JsonRpcMethod("get-rounds")]
    public List<RoundState> GetRounds(string? startTime = null, string? endTime = null)
    {
        var (startDateTime, endDateTime) = ParseInterval(startTime, endTime);
        return Analyzer.GetRoundsInInterval(startDateTime, endDateTime);
    }

    private (DateTime, DateTime) ParseInterval(string? startTime, string? endTime)
    {
        DateTime startDateTime = default;
        DateTime endDateTime = default;
        if(startTime != null && !DateTime.TryParse(startTime, out startDateTime))
        {
            throw new ArgumentException(
                $"Couldn't parse start time: {startTime}. Suggested format: YYYY-MM-DDTHH:MM:SSZ");
        }
        if(endTime != null && !DateTime.TryParse(endTime, out endDateTime))
        {
            throw new ArgumentException(
                $"Couldn't parse end time: {endTime}. Suggested format: YYYY-MM-DDTHH:MM:SSZ");
        }

        return (startDateTime, endDateTime);
    }
}