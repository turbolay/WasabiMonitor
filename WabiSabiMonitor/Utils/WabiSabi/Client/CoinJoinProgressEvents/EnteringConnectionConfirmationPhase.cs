using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Utils.WabiSabi.Client.CoinJoinProgressEvents;

public class EnteringConnectionConfirmationPhase : RoundStateChanged
{
	public EnteringConnectionConfirmationPhase(RoundState roundState, DateTimeOffset timeoutAt) : base(roundState, timeoutAt)
	{
	}
}
