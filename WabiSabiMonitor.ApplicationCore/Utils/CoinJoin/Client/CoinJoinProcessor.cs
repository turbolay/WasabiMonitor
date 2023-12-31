using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.Backend.Models.Responses;
using WabiSabiMonitor.ApplicationCore.Utils.BitcoinCore.Rpc;
using WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Transactions;
using WabiSabiMonitor.ApplicationCore.Utils.Extensions;
using WabiSabiMonitor.ApplicationCore.Utils.Helpers;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.Models;
using WabiSabiMonitor.ApplicationCore.Utils.Nito.AsyncEx;
using WabiSabiMonitor.ApplicationCore.Utils.Services;
using WabiSabiMonitor.ApplicationCore.Utils.Wallets;

namespace WabiSabiMonitor.ApplicationCore.Utils.CoinJoin.Client;

public class CoinJoinProcessor : IDisposable
{
	private volatile bool _disposedValue = false; // To detect redundant calls

	public CoinJoinProcessor(Network network, WasabiSynchronizer synchronizer, WalletManager walletManager, IRPCClient? rpc)
	{
		Synchronizer = Guard.NotNull(nameof(synchronizer), synchronizer);
		WalletManager = Guard.NotNull(nameof(walletManager), walletManager);
		Network = network;
		RpcClient = rpc;
		ProcessLock = new AsyncLock();
		Synchronizer.ResponseArrived += Synchronizer_ResponseArrivedAsync;
	}

	public WasabiSynchronizer Synchronizer { get; }
	public WalletManager WalletManager { get; }
	public Network Network { get; }
	public IRPCClient? RpcClient { get; private set; }
	private AsyncLock ProcessLock { get; }

	private async void Synchronizer_ResponseArrivedAsync(object? sender, SynchronizeResponse response)
	{
		try
		{
			using (await ProcessLock.LockAsync())
			{
				var unconfirmedCoinJoinHashes = response.UnconfirmedCoinJoins;
				if (!unconfirmedCoinJoinHashes.Any())
				{
					return;
				}

				var txsNotKnownByAWallet = WalletManager.FilterUnknownCoinjoins(unconfirmedCoinJoinHashes);

				var client = Synchronizer.HttpClientFactory.SharedWasabiClient;
				var unconfirmedCoinJoins = await client.GetTransactionsAsync(Network, txsNotKnownByAWallet, CancellationToken.None).ConfigureAwait(false);

				foreach (var tx in unconfirmedCoinJoins.Select(x => new SmartTransaction(x, Height.Mempool)))
				{
					if (RpcClient is null
						|| await TryBroadcastTransactionWithRpcAsync(tx).ConfigureAwait(false)
						|| (await RpcClient.TestAsync().ConfigureAwait(false)) is { }) // If the test throws exception then I believe it, because RPC is down and the backend is the god.
					{
						WalletManager.ProcessCoinJoin(tx);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
		}
	}

	private async Task<bool> TryBroadcastTransactionWithRpcAsync(SmartTransaction transaction)
	{
		try
		{
			if (RpcClient is null)
			{
				throw new InvalidOperationException("RpcClient is not available");
			}
			await RpcClient.SendRawTransactionAsync(transaction.Transaction).ConfigureAwait(false);
			Logger.LogInfo($"Transaction is successfully broadcasted with RPC: {transaction.GetHash()}.");

			return true;
		}
		catch
		{
			return false;
		}
	}

	#region IDisposable Support

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Synchronizer.ResponseArrived -= Synchronizer_ResponseArrivedAsync;
			}

			_disposedValue = true;
		}
	}

	// This code added to correctly implement the disposable pattern.
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		Dispose(true);
	}

	#endregion IDisposable Support
}
