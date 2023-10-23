using NBitcoin;
using WabiSabiMonitor.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.Utils.WabiSabi.Models;
using WabiSabiMonitor.Utils.WabiSabi.Models.MultipartyTransaction;

namespace WabiSabiMonitor.Data;

public static class RoundStateExtensions
{
        public static uint GetOutputsCount(this RoundState roundState)
    {
        return (uint)roundState.CoinjoinState.Outputs.Count();
    }

    public static uint GetSignaturesCount(this RoundState roundState)
    {
        try
        {
            var signingState = roundState.Assert<SigningState>();
            return (uint)signingState.Witnesses.Count;
        }
        catch
        {
            return 0;
        }
    }
    
    public static Money GetTotalInputsAmount(this RoundState roundState)
    {
        return roundState.CoinjoinState.Inputs.Sum(x => x.Amount);
    }
    
    public static IEnumerable<Money> GetInputAmounts(this RoundState roundState)
    {
        return roundState.CoinjoinState.Inputs.Select(x => x.Amount);
    }
    
    public static Money GetTotalOutputsAmount(this RoundState roundState)
    {
        return roundState.CoinjoinState.Outputs.Sum(x => x.Value);
    }
    
    public static List<Money> GetOutputsAmounts(this RoundState roundState)
    {
        return roundState.CoinjoinState.Outputs.Select(x => x.Value).ToList();
    }
    
    public static bool IsBlame(this RoundState roundState)
    {
        return roundState.BlameOf != uint256.Zero;
    }
    
    public static Money GetFee(this RoundState roundState)
    {
        return GetTotalInputsAmount(roundState) - GetTotalOutputsAmount(roundState);
    }
    
    public static int GetEstimatedVSize(this RoundState roundState)
    {
        return roundState.CoinjoinState.EstimatedVsize;
    }

    public static FeeRate GetFeeRate(this RoundState roundState)
    {
        return roundState.IsSuccess() ? 
            new FeeRate(GetFee(roundState), GetEstimatedVSize(roundState)) :
            FeeRate.Zero;
    }
    
    public static Dictionary<Money, uint> GetInputsAnonSet(this RoundState roundState)
    {
        return GetInputAmounts(roundState)
            .GroupBy(n => n)
            .ToDictionary(group => group.Key, group => (uint)group.Count());
    }
    
    public static Dictionary<Money, uint> GetOutputsAnonSet(this RoundState roundState)
    {
        return GetOutputsAmounts(roundState)
            .GroupBy(n => n)
            .ToDictionary(group => group.Key, group => (uint)group.Count());
    }

    public static Phase GetCurrentPhase(this RoundState roundState)
    {
        return roundState.Phase;
    }

    public static EndRoundState GetEndRoundState(this RoundState roundState)
    {
        return roundState.EndRoundState;
    }

    public static bool IsOngoing(this RoundState roundState)
    {
        return roundState.EndRoundState == EndRoundState.None;
    }
    
    public static bool IsSuccess(this RoundState roundState)
    {
        return roundState.EndRoundState == EndRoundState.TransactionBroadcasted;
    }

    public static bool IsCancelled(this RoundState roundState)
    {
        return roundState.EndRoundState is EndRoundState.AbortedLoadBalancing or 
            EndRoundState.AbortedNotEnoughAlices or 
            EndRoundState.AbortedNotAllAlicesConfirmed;
    }
    
    public static bool IsFailed(this RoundState roundState)
    {
        return !IsOngoing(roundState) && !IsSuccess(roundState) && !IsCancelled(roundState);
    }
    
    // This function is special: It is the only one why we need human monitor:
    // it contains this extra info during InputsRegistrationPhase.
    public static uint GetInputsCount(this RoundState roundState)
    {
        uint count;
        var hmInfosForRound = Program.DataProcessor!.LastHumanMonitor!.RoundStates.FirstOrDefault(x => x.RoundId == roundState.Id);
        if (hmInfosForRound != null)
        {
            count = (uint)hmInfosForRound.InputCount;
        }
        else
        {
            count = (uint)roundState.CoinjoinState.Inputs.Count();
        }
        return count;
    }

    public static uint GetConfirmedInputsCount(this RoundState roundState)
    {
        return (uint)roundState.CoinjoinState.Inputs.Count();
    }
    
    public static uint GetNbBanEstimation(this RoundState roundState)
    {
        if (!roundState.IsBlame())
        {
            return 0;
        }
        var blameOf = Analyzer.GetRoundsStartedSince(TimeSpan.FromHours(1)).FirstOrDefault(x => x.BlameOf == roundState.Id);
        if (blameOf is null)
        {
            return 0;
        }

        return roundState.GetConfirmedInputsCount() - blameOf.GetInputsCount();
    }
}