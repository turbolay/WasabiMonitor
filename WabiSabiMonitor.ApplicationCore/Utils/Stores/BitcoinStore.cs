using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.BitcoinP2p;
using WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Blocks;
using WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Mempool;
using WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Transactions;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.Wallets;

namespace WabiSabiMonitor.ApplicationCore.Utils.Stores;

/// <summary>
/// The purpose of this class is to safely and efficiently manage all the Bitcoin related data
/// that's being serialized to disk, like transactions, wallet files, keys, blocks, index files, etc.
/// </summary>
public class BitcoinStore
{
	/// <param name="indexStore">Not initialized index store.</param>
	/// <param name="transactionStore">Not initialized transaction store.</param>
	public BitcoinStore(
		IndexStore indexStore,
		AllTransactionStore transactionStore,
		MempoolService mempoolService,
		SmartHeaderChain smartHeaderChain,
		IRepository<uint256, Block> blockRepository)
	{
		IndexStore = indexStore;
		TransactionStore = transactionStore;
		MempoolService = mempoolService;
		SmartHeaderChain = smartHeaderChain;
		BlockRepository = blockRepository;
	}

	public IndexStore IndexStore { get; }
	public AllTransactionStore TransactionStore { get; }
	public SmartHeaderChain SmartHeaderChain { get; }
	public MempoolService MempoolService { get; }
	public IRepository<uint256, Block> BlockRepository { get; }

	/// <summary>
	/// This should not be a property, but a creator function, because it'll be cloned left and right by NBitcoin later.
	/// So it should not be assumed it's some singleton.
	/// </summary>
	public UntrustedP2pBehavior CreateUntrustedP2pBehavior() => new(MempoolService);

	public async Task InitializeAsync(CancellationToken cancel = default)
	{
		using IDisposable _ = BenchmarkLogger.Measure();

		var initTasks = new[]
		{
			IndexStore.InitializeAsync(cancel),
			TransactionStore.InitializeAsync(cancel: cancel)
		};

		await Task.WhenAll(initTasks).ConfigureAwait(false);
	}
}
