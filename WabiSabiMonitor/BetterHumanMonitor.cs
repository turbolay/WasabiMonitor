using NBitcoin;
using WabiSabiMonitor.Data;
using WabiSabiMonitor.Rpc.Models;
using WabiSabiMonitor.Utils.Extensions;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor;

public static class BetterHumanMonitor
{
    public static BetterHumanMonitorModel GetApiResponse(DateTimeOffset? start = null, DateTimeOffset? end = null)
    {

        var result = BetterHumanMonitorModel.Empty();

        BetterHumanMonitorRound CreateBetterHumanMonitorRound(RoundState round)
        {
            var blame = Analyzer.GetBlameOf(round);
            var currentPhase = round.GetCurrentPhase();
            var inputsCount = round.GetInputsCount();
            var confirmedInputsCount = round.GetInputsCount();
            var outputsCount = round.GetOutputsCount();
            var signaturesCount = round.GetSignaturesCount();
            var inputsAnonSet = round.GetInputsAnonSet();
            var outputsAnonSet = round.GetOutputsAnonSet();
            var feeRate = round.GetFeeRate();
            var currentFeesConditions = Analyzer.GetCurrentFeesConditions();
            return new(round.Id, blame, currentPhase.FriendlyName(), round.EndRoundState.FriendlyName(), inputsCount, confirmedInputsCount, outputsCount, signaturesCount, inputsAnonSet,
                outputsAnonSet, feeRate, currentFeesConditions);
        }

        var allRoundsInInterval = Analyzer.GetRoundsInInterval(start, end);

        var currentRounds = Analyzer.GetCurrentRounds();
        foreach (var current in currentRounds)
        {

            result.CurrentRounds.Add(CreateBetterHumanMonitorRound(current));
        }

        var lastPeriodRounds = allRoundsInInterval
            .Where(x => !currentRounds.Select(y => y.Id).Contains(x.Id)).ToList();

        foreach (var lastPeriodRound in lastPeriodRounds)
        {

            result.LastPeriod.Add(CreateBetterHumanMonitorRound(lastPeriodRound));
        }

        result.Analysis = Analyzer.AnalyzeRoundStates(lastPeriodRounds.ToList());

        return result;
    }

    public class BetterHumanMonitorRound
    {
        public uint256 Id { get; }
        public List<uint256> Blame { get; }
        public string CurrentPhase { get; }
        public string EndRoundState { get; }
        public uint InputsRegisteredCount { get; }
        public uint InputsConfirmedCount { get; }
        public uint OutputCount { get; }
        public uint SignaturesCount { get; }
        public Dictionary<Money, uint> InputsAnonSet { get; }
        public Dictionary<Money, uint> OutputsAnonSet { get; }
        public FeeRate? FeeRate { get; }
        public CoinJoinFeeRateMedian[] CurrentFeesConditions { get; }

        public BetterHumanMonitorRound(
            uint256 id,
            List<uint256> blame,
            string currentPhase,
            string endRoundState,
            uint inputsRegisteredCount,
            uint inputsConfirmedCount,
            uint outputCount,
            uint signaturesCount,
            Dictionary<Money, uint> inputsAnonSet,
            Dictionary<Money, uint> outputsAnonSet,
            FeeRate? feeRate,
            CoinJoinFeeRateMedian[] currentFeesConditions)
        {

            Id = id;
            Blame = blame;
            CurrentPhase = currentPhase;
            EndRoundState = endRoundState;
            InputsRegisteredCount = inputsRegisteredCount;
            InputsConfirmedCount = inputsConfirmedCount;
            OutputCount = outputCount;
            SignaturesCount = signaturesCount;
            InputsAnonSet = inputsAnonSet;
            OutputsAnonSet = outputsAnonSet;
            FeeRate = feeRate;
            CurrentFeesConditions = currentFeesConditions;
        }
    }
}