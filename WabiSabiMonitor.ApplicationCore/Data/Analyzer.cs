using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Extensions;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class Analyzer : IAnalyzer
{
    private readonly RoundDataReaderService _roundDataReaderService;
    private readonly IRoundsDataFilter _roundsDataFilter;

    public Analyzer(RoundDataReaderService roundDataReaderService, IRoundsDataFilter roundsDataFilter)
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

        var intervalEnd = roundStates.Max(x => x.InputRegistrationStart);
        var intervalStart = roundStates.Min(x => x.InputRegistrationStart);
        var intervalDuration = intervalEnd - intervalStart;
        var successes = roundStates.Where(x => x.IsSuccess()).ToList();

        double inputsPerHour =
            Math.Round(successes.Sum(x => x.GetInputsCount(_roundDataReaderService)) / intervalDuration.TotalHours, 2);
        decimal btcPerHour =
            Math.Round(
                successes.Sum(x => x.GetTotalInputsAmount().ToUnit(MoneyUnit.BTC)) /
                (decimal)intervalDuration.TotalHours, 2);
        double blameRoundsPerHour =
            Math.Round(roundStates.Sum(x => x.IsBlame() ? 1 : 0) / intervalDuration.TotalHours, 1);
        double estimatedNbOfBanPerHour =
            roundStates.Sum(x => _roundsDataFilter.GetNbBanEstimation(x)) / intervalDuration.TotalHours;
        decimal averageFeeRate = Math.Round(successes.Average(x => x.GetFeeRate().SatoshiPerByte));
        decimal nbOutputPerInput =
            Math.Round(
                (decimal)successes.Sum(x => x.GetOutputsCount()) /
                (decimal)successes.Sum(x => x.GetInputsCount(_roundDataReaderService)), 2);

        var endRoundStatePercent = roundStates.GroupBy(x => x.EndRoundState)
            .Select(x => new KeyValuePair<EndRoundState, double>(x.Key, x.Count() / (double)roundStates.Count))
            .ToDictionary(toAdd => toAdd.Key, toAdd => Math.Round(toAdd.Value * 100, 2));

        var inputsAnonSet = new Dictionary<decimal, uint>();
        var outputsAnonSet = new Dictionary<decimal, uint>();

        InitInputAnonSet(successes, inputsAnonSet);

        InitOutputAnonSet(successes, outputsAnonSet);

        var nbRounds = (decimal)successes.Count();

        var inputsAnonSetFirstQuartile =
            Math.Round(inputsAnonSet.Values.Where(x => x != 1).FirstQuartile() / nbRounds, 2);
        var inputsNbChange = Math.Round(inputsAnonSet.Count(x => x.Value == 1) / nbRounds, 2);
        var inputsMedianWithoutChange =
            Math.Round(inputsAnonSet.Where(x => x.Value != 1).Select(x => x.Value).Median() / nbRounds, 2);
        var inputsAnonSetTop = inputsAnonSet.OrderByDescending(x => x.Value).Take(3)
            .ToDictionary(x => x.Key, x => Math.Round(x.Value / nbRounds, 2));
        var inputAnonSetAnalysis = new AnonSetAnalysisPerRound(inputsMedianWithoutChange, inputsAnonSetFirstQuartile,
            inputsNbChange, inputsAnonSetTop);

        var outputsAnonSetFirstQuartile =
            Math.Round(outputsAnonSet.Values.Where(x => x != 1).FirstQuartile() / nbRounds, 2);
        var outputsNbChange = Math.Round(outputsAnonSet.Count(x => x.Value == 1) / nbRounds, 2);
        var outputsMedianWithoutChange =
            Math.Round(outputsAnonSet.Where(x => x.Value != 1).Select(x => x.Value).Median() / nbRounds, 2);
        var outputsAnonSetTop = outputsAnonSet.OrderByDescending(x => x.Value).Take(3)
            .ToDictionary(x => x.Key, x => Math.Round(x.Value / nbRounds, 2));
        var outputsAnonSetAnalysis = new AnonSetAnalysisPerRound(outputsMedianWithoutChange,
            outputsAnonSetFirstQuartile, outputsNbChange, outputsAnonSetTop);

        return new Analysis(intervalStart, intervalEnd, inputsPerHour, btcPerHour, blameRoundsPerHour,
            estimatedNbOfBanPerHour,
            averageFeeRate, nbOutputPerInput, endRoundStatePercent, inputAnonSetAnalysis, outputsAnonSetAnalysis);
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
        decimal NbChange, Dictionary<decimal, decimal> Top);

    public record Analysis(DateTimeOffset StartTime, DateTimeOffset EndTime, double InputsPerHour, decimal BtcPerHour,
        double BlameRoundsPerHour,
        double EstimatedNbOfBanPerHour, decimal AverageFeeRate, decimal NbOutputPerInput,
        Dictionary<EndRoundState, double> EndRoundStatePercent, AnonSetAnalysisPerRound InputsAnonSetPerRound,
        AnonSetAnalysisPerRound OutputsAnonSetPerRound);
}