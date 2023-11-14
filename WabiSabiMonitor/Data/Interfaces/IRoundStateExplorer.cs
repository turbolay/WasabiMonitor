using NBitcoin;
using WabiSabiMonitor.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data.Interfaces;

public interface IRoundStateExplorer
{
    uint GetOutputsCount(RoundState roundState);

    uint GetSignaturesCount(RoundState roundState);

    Money GetTotalInputsAmount(RoundState roundState);

    IEnumerable<Money> GetInputAmounts(RoundState roundState);

    Money GetTotalOutputsAmount(RoundState roundState);

    List<Money> GetOutputsAmounts(RoundState roundState);

    bool IsBlame(RoundState roundState);

    Money GetFee(RoundState roundState);

    int GetEstimatedVSize(RoundState roundState);

    FeeRate GetFeeRate(RoundState roundState);

    Dictionary<Money, uint> GetInputsAnonSet(RoundState roundState);

    Dictionary<Money, uint> GetOutputsAnonSet(RoundState roundState);

    Phase GetCurrentPhase(RoundState roundState);

    EndRoundState GetEndRoundState(RoundState roundState);

    bool IsOngoing(RoundState roundState);

    bool IsSuccess(RoundState roundState);

    bool IsCancelled(RoundState roundState);

    bool IsFailed(RoundState roundState);

    uint GetInputsCount(RoundState roundState);

    uint GetConfirmedInputsCount(RoundState roundState);

    uint GetNbBanEstimation(RoundState roundState);

}