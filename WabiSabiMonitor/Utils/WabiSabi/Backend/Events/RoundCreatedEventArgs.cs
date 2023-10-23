using NBitcoin;
using WabiSabiMonitor.Utils.WabiSabi.Backend.Rounds;

namespace WabiSabiMonitor.Utils.WabiSabi.Backend.Events;

public class RoundCreatedEventArgs : EventArgs
{
	public RoundCreatedEventArgs(uint256 roundId, RoundParameters roundParameters) : base()
	{
		RoundId = roundId;
		RoundParameters = roundParameters;
	}

	public uint256 RoundId { get; }
	public RoundParameters RoundParameters { get; }
}
