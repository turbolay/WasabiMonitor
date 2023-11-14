using WabiSabiMonitor.ApplicationCore.Utils.Wallets;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

public class CompletedEventArgs : StatusChangedEventArgs
{
	public CompletedEventArgs(IWallet wallet, CompletionStatus completionStatus)
		: base(wallet)
	{
		CompletionStatus = completionStatus;
	}

	public CompletionStatus CompletionStatus { get; }
}
