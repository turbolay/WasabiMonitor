using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client.CoinJoinProgressEvents;

public class EnteringConnectionConfirmationPhase : RoundStateChanged
{
	public EnteringConnectionConfirmationPhase(RoundState roundState, DateTimeOffset timeoutAt) : base(roundState, timeoutAt)
	{
	}
}
