using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.MultipartyTransaction;

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
        var allRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        AnalyzeRounds(allRoundsWithProblematicFailures, "All rounds");
        
        
        var smallRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.GetInputsCount() <= 200 &&
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        AnalyzeRounds(smallRoundsWithProblematicFailures, "Rounds <= 200 inputs");
        
        var middleRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.GetInputsCount() > 200 && x.GetInputsCount() < 300 &&
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        AnalyzeRounds(middleRoundsWithProblematicFailures, "Rounds > 200 inputs && < 300 inputs");
        
        var bigRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.GetInputsCount() >= 300 &&
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        AnalyzeRounds(bigRoundsWithProblematicFailures, "Rounds >= 300 inputs");
        
        var roundDestroyedRoundsWithAbortedNotEnoughAlices = ExtractRoundsCreatedByRoundDestroyer(rounds)
            .Where(x =>
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing)
            .ToList();

        var roundDestroyerRoundsWithProblematicFailures =
            roundDestroyedRoundsWithAbortedNotEnoughAlices.Where(x =>
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
                .ToList();
        
        Logger.LogInfo($"Round from round destroyer that didn't have enough inputs: {roundDestroyedRoundsWithAbortedNotEnoughAlices.Count - roundDestroyerRoundsWithProblematicFailures.Count}");

        AnalyzeRounds(roundDestroyerRoundsWithProblematicFailures, "Rounds created by round destroyer");
        await Task.Delay(-1, cancellationToken);
    }

    private List<RoundState> ExtractRoundsCreatedByRoundDestroyer(
        Dictionary<uint256, RoundDataReaderService.ProcessedRound> rounds)
    {
        // The idea here is to select the fresh rounds that have been created within 2 minutes
        // These have necessarily have been created by the RoundDestroyer.

        var freshRounds = rounds.Values
            .Select(x => x.Round)
            .Where(x => !x.IsBlame())
            .ToList();
        
        HashSet<RoundState> result = new();

        for (var i = 0; i < freshRounds.Count - 1; i++)
        { 
            var timeDifference = (freshRounds[i].InputRegistrationStart - freshRounds[i+1].InputRegistrationStart).Duration();

            if (timeDifference.TotalMinutes >= 2)
            {
                // Not a round created by round destroyer because the next one wasn't created right after
                continue;
            }
            
            result.Add(freshRounds[i]);
            result.Add(freshRounds[i+1]);
        }

        var destroyedRoundDestroyed = result.Count(x => x.EndRoundState == EndRoundState.AbortedLoadBalancing);
        Logger.LogInfo($"Round destroyed: {freshRounds.Count(x => x.EndRoundState == EndRoundState.AbortedLoadBalancing) - destroyedRoundDestroyed}");
        Logger.LogInfo($"Round from round destroyer that were destroyed again: {destroyedRoundDestroyed}");
        
        // Then we have to add the successive blame rounds to it.
        Queue<uint256> toSearchBlame = new();
        
        foreach (var id in result.Select(x => x.Id))
        {
            toSearchBlame.Enqueue(id);
        }
        
        while(toSearchBlame.Count > 0)
        {
            var currentId = toSearchBlame.Dequeue();
            var blameRoundFromCurrentId = rounds.Values.FirstOrDefault(x => x.Round.BlameOf.Equals(currentId));

            if (blameRoundFromCurrentId is null)
            {
                continue;
            }
            
            toSearchBlame.Enqueue(blameRoundFromCurrentId.Round.Id);
            result.Add(blameRoundFromCurrentId.Round);
        }

        return result.ToList();
    }
    private void AnalyzeRounds(List<RoundState> rounds, string message)
    {
        try
        {

            var roundsSucceed = rounds.Where(x => x.IsSuccess()).ToList();
            var roundsNbInputs = roundsSucceed.Average(x => x.GetInputsCount());
            var roundsAvgAnonSet = roundsSucceed.Average(y => y.GetOutputsAnonSet().Average(x => x.Value));
            var roundsAvgFreshSuccessRate =
                (double)roundsSucceed.Count(x => !x.IsBlame()) / rounds.Count(x => !x.IsBlame());
            var roundsAvgBlameSuccessRate =
                (double)roundsSucceed.Count(x => x.IsBlame()) / rounds.Count(x => x.IsBlame());

            Logger.LogInfo($"{message} \n" +
                           $"roundsSucceed: {roundsSucceed.Count}\n" +
                           $"roundsNpInputs: {roundsNbInputs}\n" +
                           $"roundsAvgAnonSet: {roundsAvgAnonSet}\n" +
                           $"roundsAvgFreshSuccessRate: {roundsAvgFreshSuccessRate}\n" +
                           $"roundsAvgBlameSuccessRate: {roundsAvgBlameSuccessRate}\n" +
                           $"\n");
        }
        catch (Exception)
        {
            Logger.LogWarning($"{message} had no element");
        }

    }
}