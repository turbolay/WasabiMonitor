using WabiSabiMonitor.ApplicationCore.Utils.Wallets;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client;

public interface IWalletProvider
{
	Task<IEnumerable<IWallet>> GetWalletsAsync();
}
