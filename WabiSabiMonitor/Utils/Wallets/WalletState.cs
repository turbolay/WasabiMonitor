namespace WabiSabiMonitor.Utils.Wallets;

public enum WalletState
{
	Uninitialized,
	WaitingForInit,
	Initialized,
	Starting,
	Started,
	Stopping,
	Stopped
}
