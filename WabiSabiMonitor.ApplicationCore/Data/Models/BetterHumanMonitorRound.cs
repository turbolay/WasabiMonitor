using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Data.Models;

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