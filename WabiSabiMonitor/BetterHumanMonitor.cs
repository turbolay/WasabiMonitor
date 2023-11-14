using NBitcoin;
using WabiSabiMonitor.Data;
using WabiSabiMonitor.Data.Interfaces;
using WabiSabiMonitor.Rpc.Models;
using WabiSabiMonitor.Utils.Extensions;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor;

public class BetterHumanMonitor
{
    private readonly IRoundsDataFilter _roundDataFilter;
    private readonly RoundDataProcessor _roundDataProcessor;
    private readonly IAnalyzer _analyzer;

    public BetterHumanMonitor(IRoundsDataFilter roundDataFilter, RoundDataProcessor roundDataProcessor,
        IAnalyzer analyzer)
    {
        _roundDataFilter = roundDataFilter;
        _roundDataProcessor = roundDataProcessor;
        _analyzer = analyzer;
    }

    public BetterHumanMonitorModel GetApiResponse(DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var result = BetterHumanMonitorModel.Empty();

        BetterHumanMonitorRound CreateBetterHumanMonitorRound(RoundState round)
        {
            var blame = _roundDataFilter.GetBlameOf(round);
            var currentPhase = round.GetCurrentPhase();
            var inputsCount = round.GetInputsCount();
            var confirmedInputsCount = round.GetInputsCount();
            var outputsCount = round.GetOutputsCount();
            var signaturesCount = round.GetSignaturesCount();
            var inputsAnonSet = round.GetInputsAnonSet();
            var outputsAnonSet = round.GetOutputsAnonSet();
            var feeRate = round.GetFeeRate();
            var currentFeesConditions = _roundDataProcessor.GetCurrentFeesConditions();
            return new(round.Id, blame, currentPhase.FriendlyName(), round.EndRoundState.FriendlyName(), inputsCount,
                confirmedInputsCount, outputsCount, signaturesCount, inputsAnonSet,
                outputsAnonSet, feeRate, currentFeesConditions);
        }

        var allRoundsInInterval = _roundDataFilter.GetRoundsInInterval(start, end);

        var currentRounds = _roundDataFilter.GetCurrentRounds();
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

        result.Analysis = _analyzer.AnalyzeRoundStates(lastPeriodRounds.ToList());

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