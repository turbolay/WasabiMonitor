using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Rpc.Models;
using WabiSabiMonitor.ApplicationCore.Utils.Extensions;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore;

public class BetterHumanMonitor : IBetterHumanMonitor
{
    private readonly IRoundsDataFilter _roundDataFilter;
    private readonly IRoundDataProcessor _roundDataProcessor;
    private readonly IAnalyzer _analyzer;
    private readonly IRoundDataReaderService _roundDataReaderService;

    public BetterHumanMonitor(IRoundsDataFilter roundDataFilter, IRoundDataProcessor roundDataProcessor,
        IAnalyzer analyzer, IRoundDataReaderService roundDataReaderService)
    {
        _roundDataFilter = roundDataFilter;
        _roundDataProcessor = roundDataProcessor;
        _analyzer = analyzer;
        _roundDataReaderService = roundDataReaderService;
    }

    public BetterHumanMonitorModel GetApiResponse(TimeSpan? durationNullable = null)
    {
        var duration = durationNullable ?? TimeSpan.FromHours(12);
        DateTimeOffset start = DateTimeOffset.UtcNow - duration;
        
        var result = BetterHumanMonitorModel.Empty();

        BetterHumanMonitorRound CreateBetterHumanMonitorRound(RoundState round)
        {
            var blame = _roundDataFilter.GetBlameOf(round);
            var currentPhase = round.GetCurrentPhase();
            var inputsCount = round.GetInputsCount(_roundDataReaderService);
            var confirmedInputsCount = round.GetInputsCount(_roundDataReaderService);
            var outputsCount = round.GetOutputsCount();
            var signaturesCount = round.GetSignaturesCount();
            var inputsAnonSet = round.GetInputsAnonSet();
            var outputsAnonSet = round.GetOutputsAnonSet();
            var feeRate = round.GetFeeRate();
            var currentFeesConditions = _roundDataProcessor.GetCurrentFeesConditions();
           
            return new(round.Id,
                blame,
                currentPhase.FriendlyName(),
                round.EndRoundState.FriendlyName(),
                inputsCount,
                confirmedInputsCount,
                outputsCount,
                signaturesCount,
                inputsAnonSet,
                outputsAnonSet,
                feeRate,
                currentFeesConditions);
        }

        var allRoundsInInterval = _roundDataFilter.GetRoundsInInterval(start, null);

        var currentRounds = _roundDataFilter.GetCurrentRounds();
        foreach (var current in currentRounds)
        {
            result.CurrentRounds.Add(CreateBetterHumanMonitorRound(current));
        }

        var lastPeriodRounds = allRoundsInInterval.Where(x => !currentRounds.Select(y => y.Id).Contains(x.Id)).ToList();

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