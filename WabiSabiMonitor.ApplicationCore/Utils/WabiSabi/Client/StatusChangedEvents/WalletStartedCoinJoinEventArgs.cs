using WabiSabiMonitor.ApplicationCore.Utils.Wallets;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class WalletStartedCoinJoinEventArgs : StatusChangedEventArgs
{
	public WalletStartedCoinJoinEventArgs(IWallet wallet) : base(wallet)
	{
	}
}
