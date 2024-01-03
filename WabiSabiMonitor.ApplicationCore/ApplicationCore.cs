using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore;

public class ApplicationCore
{
    private readonly Scraper _roundStatusScraper;
    private readonly IRoundDataReaderService _dataReader;
    private readonly IRpcServerController _rpcServerController;
    private readonly IRoundsDataFilter _roundDataFilter;
    private readonly IAnalyzer _analyzer;
    private readonly IBetterHumanMonitor _betterHumanMonitor;
    private readonly IRoundDataProcessor _dataProcessor;

    public ApplicationCore(Scraper roundStatusScraper, IRoundDataReaderService dataReader,
        IRpcServerController rpcServerController, IRoundsDataFilter roundDataFilter, IAnalyzer analyzer, IBetterHumanMonitor betterHumanMonitor, IRoundDataProcessor dataProcessor)
    {
        _roundStatusScraper = roundStatusScraper;
        _dataReader = dataReader;
        _rpcServerController = rpcServerController;
        _roundDataFilter = roundDataFilter;
        _analyzer = analyzer;
        _betterHumanMonitor = betterHumanMonitor;
        _dataProcessor = dataProcessor;
    }

    private async Task<Dictionary<uint256, DateTime>> GetConfirmationTime(List<uint256> rounds)
    {
        var result = new Dictionary<uint256, DateTime>();
        /* TODO: TEST, FIX, FINISH
        var client = new HttpClient();
        foreach (var txid in rounds)
        {
            var url = $"https://mempool.space/api/tx/{txid}/status";
            try
            {
                await Task.Delay(1000);
                var responseBody = await client.GetStringAsync(url);
                var json = JsonConvert.DeserializeObject(responseBody);
                result.Add(txid, new DateTime(responseBody["block_time"]);
            }
            catch (Exception e)
            {
                // Exception suppression
            }
        }*/
        return result;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        Logger.InitializeDefaults("./logs.txt");

        var rounds = _dataReader.Rounds;

        // TODO: RESTORE CONFIRMATION TIME
        
        // TODO: POPULATE MISSING CONFIRMATION TIME
        await GetConfirmationTime(rounds.Where(x => x.Value.Round.IsSuccess()).Select(x => x.Key).ToList());
        
        // TODO: SAVE CONFIRMATION TIME
        
        var bigRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.GetInputsCount() >= 300 &&
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        var bigRoundsSucceed = bigRoundsWithProblematicFailures.Where(x => x.IsSuccess()).ToList();
        
        var bigRoundsNpInputs = bigRoundsSucceed.Average(x => x.GetInputsCount());
        var bigRoundsAvgAnonScore = bigRoundsSucceed.Average(y => y.GetOutputsAnonSet().Average(x => x.Value));
        var bigRoundsAvgFreshSuccessRate = (double)bigRoundsSucceed.Count(x => !x.IsBlame()) / bigRoundsWithProblematicFailures.Count(x => !x.IsBlame());
        var bigRoundsAvgBlameSuccessRate = (double)bigRoundsSucceed.Count(x => x.IsBlame()) / bigRoundsWithProblematicFailures.Count(x => x.IsBlame());
        
        var smallRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.GetInputsCount() <= 180 &&
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        var smallRoundsSucceed = smallRoundsWithProblematicFailures.Where(x => x.IsSuccess()).ToList();
        var smallRoundsNbInputs = smallRoundsSucceed.Average(x => x.GetInputsCount());
        var smallRoundsAvgAnonScore = smallRoundsSucceed.Average(y => y.GetOutputsAnonSet().Average(x => x.Value));
        var smallRoundsAvgFreshSuccessRate = (double)smallRoundsSucceed.Count(x => !x.IsBlame()) / smallRoundsWithProblematicFailures.Count(x => !x.IsBlame());
        var smallRoundsAvgBlameSuccessRate = (double)smallRoundsSucceed.Count(x => x.IsBlame()) / smallRoundsWithProblematicFailures.Count(x => x.IsBlame());

        Logger.LogInfo($"" +
                       $"bigRoundsNpInputs: {bigRoundsNpInputs}\n" +
                       $"bigRoundsAvgAnonScore: {bigRoundsAvgAnonScore}\n" +
                       $"bigRoundsAvgFreshSuccessRate: {bigRoundsAvgFreshSuccessRate}\n" +
                       $"bigRoundsAvgBlameSuccessRate: {bigRoundsAvgBlameSuccessRate}\n" +
                       $"\n" +
                       $"smallRoundsNbInputs: {smallRoundsNbInputs}\n" +
                       $"smallRoundsAvgAnonScore: {smallRoundsAvgAnonScore}\n" +
                       $"smallRoundsAvgFreshSuccessRate: {smallRoundsAvgFreshSuccessRate}\n" +
                       $"smallRoundsAvgBlameSuccessRate: {smallRoundsAvgBlameSuccessRate}");
        
        await Task.Delay(-1, cancellationToken);
    }
}