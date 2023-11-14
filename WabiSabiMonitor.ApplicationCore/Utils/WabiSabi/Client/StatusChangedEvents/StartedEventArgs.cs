using WabiSabiMonitor.ApplicationCore.Utils.Wallets;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class StartedEventArgs : StatusChangedEventArgs
{
	public StartedEventArgs(IWallet wallet, TimeSpan registrationTimeout)
		: base(wallet)
	{
		RegistrationTimeout = registrationTimeout;
	}

	public TimeSpan RegistrationTimeout { get; }
}
