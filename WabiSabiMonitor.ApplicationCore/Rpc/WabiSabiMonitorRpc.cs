using System.Globalization;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Rpc.Models;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.Rpc;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Rpc;

public class WabiSabiMonitorRpc : IJsonRpcService
{
    private readonly IRoundsDataFilter _filter;
    private readonly IAnalyzer _analyzer;
    private readonly IBetterHumanMonitor _betterHumanMonitor;

    public WabiSabiMonitorRpc(IRoundsDataFilter filter, IAnalyzer analyzer, IBetterHumanMonitor betterHumanMonitor)
    {
        _filter = filter;
        _analyzer = analyzer;
        _betterHumanMonitor = betterHumanMonitor;
    }

    [JsonRpcMethod("echo")]
    public string HelloWorld()
    {
        var message = "Hello world!";
        Logger.LogInfo(message);
        return message;
    }

    [JsonRpcMethod("better-human-monitor")]
    public BetterHumanMonitorModel GetBetterHumanMonitor() => _betterHumanMonitor.GetApiResponse();

    [JsonRpcMethod("get-analysis")]
    public Analyzer.Analysis? GetAnalysis(string? startTime = null, string? endTime = null)
    {
        var (startDateTime, endDateTime) = ParseInterval(startTime, endTime);
        if (startTime is null && endTime is null)
        {
            startDateTime = DateTime.UtcNow - TimeSpan.FromHours(12);
        }

        return _analyzer.AnalyzeRoundStates(_filter.GetRoundsInInterval(startDateTime, endDateTime));
    }

    [JsonRpcMethod("get-rounds")]
    public List<RoundState> GetRounds(string? startTime = null, string? endTime = null)
    {
        var (startDateTime, endDateTime) = ParseInterval(startTime, endTime);
        return _filter.GetRoundsInInterval(startDateTime, endDateTime);
    }

    private static (DateTime, DateTime) ParseInterval(string? startTime, string? endTime)
    {
        DateTime startDateTime = default;
        DateTime endDateTime = DateTime.UtcNow;

        string[] formats = { "yyyy-MM-ddTHH:mm:ssZ", "MM/dd/yyyy HH:mm:ss" };

        if (startTime is not null && !DateTime.TryParseExact(startTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDateTime))
        {
            throw new ArgumentException($"Couldn't parse start time: {startTime}. Suggested formats: {string.Join(", ", formats)}");
        }

        if (endTime is not null && !DateTime.TryParseExact(endTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDateTime))
        {
            throw new ArgumentException($"Couldn't parse end time: {endTime}. Suggested formats: {string.Join(", ", formats)}");
        }

        return (startDateTime, endDateTime);
    }
}