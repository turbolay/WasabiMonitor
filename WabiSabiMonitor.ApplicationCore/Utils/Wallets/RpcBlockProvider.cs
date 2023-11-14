using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.BitcoinCore.Rpc;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;

namespace WabiSabiMonitor.ApplicationCore.Utils.Wallets;

public class RpcBlockProvider : IBlockProvider
{
	public RpcBlockProvider(IRPCClient rpcClient)
	{
		RpcClient = rpcClient;
	}

	private IRPCClient RpcClient { get; }

	public async Task<Block?> TryGetBlockAsync(uint256 hash, CancellationToken cancellationToken)
	{
		try
		{
			return await RpcClient.GetBlockAsync(hash, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Logger.LogDebug(ex);
			return null;
		}
	}
}
