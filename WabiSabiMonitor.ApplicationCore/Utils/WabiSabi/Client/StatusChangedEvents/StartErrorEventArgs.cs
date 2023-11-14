using WabiSabiMonitor.ApplicationCore.Utils.Wallets;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class StartErrorEventArgs : StatusChangedEventArgs
{
	public StartErrorEventArgs(IWallet wallet, CoinjoinError error)
		: base(wallet)
	{
		Error = error;
	}

	public CoinjoinError Error { get; }
}
