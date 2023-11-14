using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Keys;
using WabiSabiMonitor.ApplicationCore.Utils.Crypto;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client;

public interface IKeyChain
{
	OwnershipProof GetOwnershipProof(IDestination destination, CoinJoinInputCommitmentData committedData);

	Transaction Sign(Transaction transaction, Coin coin, PrecomputedTransactionData precomputeTransactionData);

	void TrySetScriptStates(KeyState state, IEnumerable<Script> scripts);
}
