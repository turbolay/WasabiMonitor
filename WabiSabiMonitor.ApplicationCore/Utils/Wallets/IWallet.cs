using WabiSabiMonitor.ApplicationCore.Utils.Blockchain.TransactionOutputs;
using WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Transactions;
using WabiSabiMonitor.ApplicationCore.Utils.Models;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client;

namespace WabiSabiMonitor.ApplicationCore.Utils.Wallets;

public interface IWallet
{
	string WalletName { get; }
	bool IsUnderPlebStop { get; }
	bool IsMixable { get; }

	/// <summary>
	/// Watch only wallets have no key chains.
	/// </summary>
	IKeyChain? KeyChain { get; }

	IDestinationProvider DestinationProvider { get; }
	int AnonScoreTarget { get; }
	bool ConsolidationMode { get; }
	TimeSpan FeeRateMedianTimeFrame { get; }
	bool RedCoinIsolation { get; }
	CoinjoinSkipFactors CoinjoinSkipFactors { get; }

	Task<bool> IsWalletPrivateAsync();

	Task<IEnumerable<SmartCoin>> GetCoinjoinCoinCandidatesAsync();

	Task<IEnumerable<SmartTransaction>> GetTransactionsAsync();
}
