using NBitcoin;
using WabiSabiMonitor.Utils.WabiSabi.Backend.Rounds;

namespace WabiSabiMonitor.Utils.WabiSabi.Backend.Events;

public class RoundPhaseChangedEventArgs : EventArgs
{
	public RoundPhaseChangedEventArgs(uint256 roundId, Phase phase) : base()
	{
		RoundId = roundId;
		Phase = phase;
	}

	public uint256 RoundId { get; }
	public Phase Phase { get; }
}
