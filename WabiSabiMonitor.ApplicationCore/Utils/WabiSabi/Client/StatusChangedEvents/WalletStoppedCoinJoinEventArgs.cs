using WabiSabiMonitor.ApplicationCore.Utils.Wallets;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class WalletStoppedCoinJoinEventArgs : StatusChangedEventArgs
{
	public WalletStoppedCoinJoinEventArgs(IWallet wallet) : base(wallet)
	{
	}
}
