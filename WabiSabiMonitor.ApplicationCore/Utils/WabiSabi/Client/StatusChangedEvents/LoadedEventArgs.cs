using WabiSabiMonitor.ApplicationCore.Utils.Wallets;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class LoadedEventArgs : StatusChangedEventArgs
{
	public LoadedEventArgs(IWallet wallet)
		: base(wallet)
	{
	}
}
