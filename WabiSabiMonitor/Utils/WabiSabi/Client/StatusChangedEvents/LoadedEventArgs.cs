using WabiSabiMonitor.Utils.Wallets;

namespace WabiSabiMonitor.Utils.WabiSabi.Client.StatusChangedEvents;

public class LoadedEventArgs : StatusChangedEventArgs
{
	public LoadedEventArgs(IWallet wallet)
		: base(wallet)
	{
	}
}
