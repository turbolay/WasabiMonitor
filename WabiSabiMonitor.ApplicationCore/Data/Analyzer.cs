using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Extensions;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class Analyzer : IAnalyzer
{
    private readonly IRoundDataReaderService _roundDataReaderService;
    private readonly IRoundsDataFilter _roundsDataFilter;

    public Analyzer(IRoundDataReaderService roundDataReaderService, IRoundsDataFilter roundsDataFilter)
    {
        _roundDataReaderService = roundDataReaderService;
        _roundsDataFilter = roundsDataFilter;
    }

    public Analysis? AnalyzeRoundStates(List<RoundState> roundStates)
    {
        if (!roundStates.Any())
        {
            return null;
        }

        var (Start, End) = GetInterval(roundStates);
        var intervalDuration = (End - Start);

        decimal averageFeeRate = 0;
        decimal nbOutputPerInput = 0;
        AnonSetAnalysisPerRound inputAnonSetAnalysis = new();
        AnonSetAnalysisPerRound outputAnonSetAnalysis = new();

        var successes = roundStates.Where(x => x.IsSuccess()).ToList();

        var inputsAnonSet = new Dictionary<decimal, uint>();
        var outputsAnonSet = new Dictionary<decimal, uint>();

        InitInputAnonSet(successes, inputsAnonSet);
        InitOutputAnonSet(successes, outputsAnonSet);

        if (successes.Count != 0)
        {
            averageFeeRate = CalculateAverageFeeRate(successes);
            nbOutputPerInput = CalculateNbOutputPerInput(successes);
            inputAnonSetAnalysis = CalculateAnonSetAnalysis(inputsAnonSet);
            outputAnonSetAnalysis = CalculateAnonSetAnalysis(outputsAnonSet);
        }
        var inputsPerHour = CalculateInputsPerHour(successes, intervalDuration);
        var btcPerHour = CalculateBtcPerHour(successes, intervalDuration);
        var blameRoundsPerHour = CalculateBlameRoundsPerHour(roundStates, intervalDuration);
        var endRoundStatePercent = CalculateEndRoundStatePercent(roundStates);

        double estimatedNbOfBanPerHour =
            roundStates.Sum(x => _roundsDataFilter.GetNbBanEstimation(x)) / intervalDuration.TotalHours;

        return new Analysis(
            Start,
            End,
            inputsPerHour,
            btcPerHour,
            blameRoundsPerHour,
            estimatedNbOfBanPerHour,
            averageFeeRate,
            nbOutputPerInput,
            endRoundStatePercent,
            inputAnonSetAnalysis,
            outputAnonSetAnalysis
            );
    }

    private AnonSetAnalysisPerRound CalculateAnonSetAnalysis(Dictionary<decimal, uint> anonSet)
    {
        const int uniqueValues = 1;

        List<KeyValuePair<decimal, uint>> anonSetFiltered = anonSet.Where(x => x.Value != uniqueValues).ToList();
        uint firstQuartile = anonSetFiltered.Select(x => x.Value).FirstQuartile();
        uint medianWithoutChange = anonSetFiltered.Select(x => x.Value).Median();
        int nbChange = anonSet.Count(x => x.Value == uniqueValues);

        Dictionary<decimal, uint> anonSetTop = anonSet.OrderByDescending(x => x.Value).Take(3)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        return new AnonSetAnalysisPerRound(
            medianWithoutChange,
            firstQuartile,
            nbChange,
            anonSetTop);
    }

    private decimal CalculateNbOutputPerInput(List<RoundState> successes)
    {
        return Math.Round(
            (decimal)successes.Sum(x => x.GetOutputsCount()) /
            successes.Sum(x => x.GetInputsCount(_roundDataReaderService)), 2);
    }

    private decimal CalculateBtcPerHour(List<RoundState> successes, TimeSpan intervalDuration)
    {
        return Math.Round(
            successes.Sum(x => x.GetTotalInputsAmount().ToUnit(MoneyUnit.BTC)) / (decimal)intervalDuration.TotalHours,
            2);
    }

    private double CalculateInputsPerHour(List<RoundState> successes, TimeSpan intervalDuration)
    {
        return (double)Math.Round(
            successes.Sum(x => x.GetInputsCount(_roundDataReaderService)) / (decimal)intervalDuration.TotalHours, 2);
    }

    private double CalculateBlameRoundsPerHour(List<RoundState> roundStates, TimeSpan intervalDuration)
    {
        return (double)Math.Round(roundStates.Sum(x => x.IsBlame() ? 1 : 0) / (decimal)intervalDuration.TotalHours, 1);
    }

    private decimal CalculateAverageFeeRate(List<RoundState> successes)
    {
        return Math.Round(successes.Average(x => x.GetFeeRate().SatoshiPerByte));
    }

    private (DateTimeOffset Start, DateTimeOffset End) GetInterval(List<RoundState> roundStates)
    {
        var intervalEnd = roundStates.Max(x => x.InputRegistrationStart);
        var intervalStart = roundStates.Min(x => x.InputRegistrationStart);
        return (intervalStart, intervalEnd);
    }

    private Dictionary<EndRoundState, double> CalculateEndRoundStatePercent(List<RoundState> roundStates)
    {
        return roundStates
            .GroupBy(x => x.EndRoundState)
            .Select(x => new KeyValuePair<EndRoundState, double>(x.Key, x.Count() / (double)roundStates.Count))
            .ToDictionary(toAdd => toAdd.Key, toAdd => Math.Round(toAdd.Value * 100, 2));
    }

    private static void InitOutputAnonSet(List<RoundState> successes, Dictionary<decimal, uint> outputsAnonSet)
    {
        foreach (var dictionary in successes.Select(x => x.GetOutputsAnonSet()))
        {
            foreach (var kvp in dictionary)
            {
                if (outputsAnonSet.ContainsKey(kvp.Key.ToUnit(MoneyUnit.BTC)))
                {
                    outputsAnonSet[kvp.Key.ToUnit(MoneyUnit.BTC)] += kvp.Value;
                }
                else
                {
                    outputsAnonSet.Add(kvp.Key.ToUnit(MoneyUnit.BTC), kvp.Value);
                }
            }
        }
    }

    private static void InitInputAnonSet(List<RoundState> successes, Dictionary<decimal, uint> inputsAnonSet)
    {
        foreach (var dictionary in successes.Select(x => x.GetInputsAnonSet()))
        {
            foreach (var kvp in dictionary)
            {
                if (inputsAnonSet.ContainsKey(kvp.Key.ToUnit(MoneyUnit.BTC)))
                {
                    inputsAnonSet[kvp.Key.ToUnit(MoneyUnit.BTC)] += kvp.Value;
                }
                else
                {
                    inputsAnonSet.Add(kvp.Key.ToUnit(MoneyUnit.BTC), kvp.Value);
                }
            }
        }
    }

    public record AnonSetAnalysisPerRound(decimal MedianWithoutChange, decimal FirstQuartileWithoutChange,
        decimal NbChange, Dictionary<decimal, uint> Top)
    {
        public AnonSetAnalysisPerRound() : this(0, 0, 0, new Dictionary<decimal, uint>())
        {
        }
    }

    public record Analysis(DateTimeOffset StartTime,
        DateTimeOffset EndTime,
        double InputsPerHour,
        decimal BtcPerHour,
        double BlameRoundsPerHour,
        double EstimatedNbOfBanPerHour,
        decimal AverageFeeRate,
        decimal NbOutputPerInput,
        Dictionary<EndRoundState, double> EndRoundStatePercent,
        AnonSetAnalysisPerRound InputsAnonSetPerRound,
        AnonSetAnalysisPerRound OutputsAnonSetPerRound);
}