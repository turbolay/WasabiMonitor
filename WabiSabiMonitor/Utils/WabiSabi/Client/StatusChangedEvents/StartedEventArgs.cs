using WabiSabiMonitor.Utils.Wallets;

namespace WabiSabiMonitor.Utils.WabiSabi.Client.StatusChangedEvents;

public class StartedEventArgs : StatusChangedEventArgs
{
	public StartedEventArgs(IWallet wallet, TimeSpan registrationTimeout)
		: base(wallet)
	{
		RegistrationTimeout = registrationTimeout;
	}

	public TimeSpan RegistrationTimeout { get; }
}
