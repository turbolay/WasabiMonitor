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

        List<(Money PlannedFee, FeeRate PlannedFeeRate, Money ActualFee, FeeRate ActualFeeRate)> feeAnalysis = new();
        
        foreach (var round in rounds.Values.Where(x => x.Round.IsSuccess()).Select(x => x.Round))
        {
            var roundPlannedFeeRate = round
                .CoinjoinState
                .Events
                .OfType<RoundCreated>()
                .First()
                .RoundParameters
                .MiningFeeRate;

            var tx = ((SigningState)round.CoinjoinState).CreateTransaction();
            var txRealVSize = tx.GetVirtualSize();
            
            feeAnalysis.Add((
                PlannedFee: roundPlannedFeeRate.GetFee(txRealVSize),
                PlannedFeeRate: roundPlannedFeeRate,
                ActualFee: round.GetFee(),
                ActualFeeRate: round.GetFeeRate(txRealVSize)));
        }
        Logger.LogInfo($"{feeAnalysis.Count()}");
        Logger.LogInfo($"Avg planned fee rate: {feeAnalysis.Select(x => x.PlannedFeeRate).Average(x => x.SatoshiPerByte):0.##}");
        Logger.LogInfo($"Percentage rounds fee rate changed: {feeAnalysis.Select(x => x.ActualFee - x.PlannedFee).Count(x => x > Money.Satoshis(1)) / (decimal)feeAnalysis.Count() * 100:0.##} %");
        Logger.LogInfo($"Avg fee increased per round where fee changed: {feeAnalysis.Select(x => x.ActualFee - x.PlannedFee).Where(x => x > Money.Satoshis(1)).Average(x => x):0.##}");
        Logger.LogInfo($"AVG FEE INCREASED PER ROUND: {feeAnalysis.Select(x => x.ActualFee - x.PlannedFee).Average(x => x):0.##}");
        
        // TODO: RESTORE CONFIRMATION TIME
        
        // TODO: POPULATE MISSING CONFIRMATION TIME
        await GetConfirmationTime(rounds.Where(x => x.Value.Round.IsSuccess()).Select(x => x.Key).ToList());
        
        // TODO: SAVE CONFIRMATION TIME
        var allRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        AnalyzeRounds(allRoundsWithProblematicFailures, "All rounds");
        
        
        var smallRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.GetInputsCount() <= 200 &&
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        AnalyzeRounds(smallRoundsWithProblematicFailures, "Rounds <= 200 inputs");
        
        var middleRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.GetInputsCount() > 200 && x.GetInputsCount() < 300 &&
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        AnalyzeRounds(middleRoundsWithProblematicFailures, "Rounds > 200 inputs && < 300 inputs");
        
        var bigRoundsWithProblematicFailures = rounds.Values
            .Select(x => x.Round)
            .Where(x =>
                x.GetInputsCount() >= 300 &&
                x.EndRoundState != EndRoundState.None &&
                x.EndRoundState != EndRoundState.AbortedLoadBalancing &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices &&
                x.EndRoundState != EndRoundState.AbortedNotEnoughAlices)
            .ToList();
        AnalyzeRounds(bigRoundsWithProblematicFailures, "Rounds >= 300 inputs");
        
        await Task.Delay(-1, cancellationToken);
    }

    private void AnalyzeRounds(List<RoundState>? rounds, string message)
    {
        try
        {

            var roundsSucceed = rounds.Where(x => x.IsSuccess()).ToList();
            var roundsNbInputs = roundsSucceed.Average(x => x.GetInputsCount());
            var roundsAvgAnonScore = roundsSucceed.Average(y => y.GetOutputsAnonSet().Average(x => x.Value));
            var roundsAvgFreshSuccessRate =
                (double)roundsSucceed.Count(x => !x.IsBlame()) / rounds.Count(x => !x.IsBlame());
            var roundsAvgBlameSuccessRate =
                (double)roundsSucceed.Count(x => x.IsBlame()) / rounds.Count(x => x.IsBlame());

            Logger.LogInfo($"{message} \n" +
                           $"roundsSucceed: {roundsSucceed.Count}\n" +
                           $"roundsNpInputs: {roundsNbInputs}\n" +
                           $"roundsAvgAnonScore: {roundsAvgAnonScore}\n" +
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