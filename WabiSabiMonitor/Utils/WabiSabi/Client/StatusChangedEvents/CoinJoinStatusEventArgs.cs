using WabiSabiMonitor.Utils.WabiSabi.Client.CoinJoinProgressEvents;
using WabiSabiMonitor.Utils.Wallets;

namespace WabiSabiMonitor.Utils.WabiSabi.Client.StatusChangedEvents;

public class CoinJoinStatusEventArgs : StatusChangedEventArgs
{
	public CoinJoinStatusEventArgs(IWallet wallet, CoinJoinProgressEventArgs coinJoinProgressEventArgs) : base(wallet)
	{
		CoinJoinProgressEventArgs = coinJoinProgressEventArgs;
	}

	public CoinJoinProgressEventArgs CoinJoinProgressEventArgs { get; }
}
