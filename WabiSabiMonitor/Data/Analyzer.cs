using NBitcoin;
using WabiSabiMonitor.Data.Interfaces;
using WabiSabiMonitor.Utils.Extensions;
using WabiSabiMonitor.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data;

public class Analyzer : IAnalyzer
{
    private RoundDataReader _roundDataReader;

    public Analyzer(RoundDataReader roundDataReader)
    {
        _roundDataReader = roundDataReader;
    }

    public List<RoundState> GetCurrentRounds() =>
        _roundDataReader.GetRounds(x => x.EndRoundState == EndRoundState.None);

    public List<RoundState> GetRoundsStartedSince(DateTimeOffset since, Func<RoundState, bool>? predicate = null) =>
        _roundDataReader.GetRounds(x => x.InputRegistrationStart >= since && (predicate?.Invoke(x) ?? true));

    public List<RoundState> GetRoundsStartedSince(TimeSpan since, Func<RoundState, bool>? predicate = null) =>
        GetRoundsStartedSince(DateTimeOffset.UtcNow - since, predicate);

    public List<RoundState> GetRoundsFinishedSince(DateTimeOffset since, Func<RoundState, bool>? predicate = null) =>
        _roundDataReader.GetRounds(x => !x.IsOngoing() && (predicate?.Invoke(x) ?? true),
            x => x.LastUpdate >= since);

    public List<RoundState> GetRoundsInInterval(DateTimeOffset? start, DateTimeOffset? end, Func<RoundState, bool>? predicate = null) =>
        _roundDataReader.GetRounds(x =>
            (start == default || x.InputRegistrationStart.DateTime >= start) &&
            (end == default || x.InputRegistrationStart.DateTime <= end) && (predicate?.Invoke(x) ?? true));


    public List<uint256> GetBlameOf(RoundState roundState)
    {
        if (roundState.BlameOf == uint256.Zero)
        {
            return new List<uint256>();
        }

        var result = new List<uint256>();
        var toSearchId = roundState.BlameOf;
        while (true)
        {
            var blameOf = GetRoundsStartedSince(TimeSpan.FromHours(1)).FirstOrDefault(x => x.Id == toSearchId);
            if (blameOf is null)
            {
                result.Add(roundState.BlameOf);
                break;
            }

            result.Add(toSearchId);
            toSearchId = blameOf.BlameOf;
        }

        return result;
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

        double inputsPerHour = Math.Round(successes.Sum(x => x.GetInputsCount()) / intervalDuration.TotalHours, 2);
        decimal btcPerHour =
            Math.Round(
                successes.Sum(x => x.GetTotalInputsAmount().ToUnit(MoneyUnit.BTC)) /
                (decimal)intervalDuration.TotalHours, 2);
        double blameRoundsPerHour =
            Math.Round(roundStates.Sum(x => x.IsBlame() ? 1 : 0) / intervalDuration.TotalHours, 1);
        double estimatedNbOfBanPerHour =
            roundStates.Sum(x => x.GetNbBanEstimation()) / intervalDuration.TotalHours;
        decimal averageFeeRate = Math.Round(successes.Average(x => x.GetFeeRate().SatoshiPerByte));
        decimal nbOutputPerInput =
            Math.Round(
                (decimal)successes.Sum(x => x.GetOutputsCount()) / (decimal)successes.Sum(x => x.GetInputsCount()), 2);

        var endRoundStatePercent = roundStates.GroupBy(x => x.EndRoundState)
            .Select(x => new KeyValuePair<EndRoundState, double>(x.Key, x.Count() / (double)roundStates.Count))
            .ToDictionary(toAdd => toAdd.Key, toAdd => Math.Round(toAdd.Value * 100, 2));

        var inputsAnonSet = new Dictionary<decimal, uint>();
        var outputsAnonSet = new Dictionary<decimal, uint>();

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

    public record AnonSetAnalysisPerRound(decimal MedianWithoutChange, decimal FirstQuartileWithoutChange,
        decimal NbChange, Dictionary<decimal, decimal> Top);

    public record Analysis(DateTimeOffset StartTime, DateTimeOffset EndTime, double InputsPerHour, decimal BtcPerHour,
        double BlameRoundsPerHour,
        double EstimatedNbOfBanPerHour, decimal AverageFeeRate, decimal NbOutputPerInput,
        Dictionary<EndRoundState, double> EndRoundStatePercent, AnonSetAnalysisPerRound InputsAnonSetPerRound,
        AnonSetAnalysisPerRound OutputsAnonSetPerRound);
}