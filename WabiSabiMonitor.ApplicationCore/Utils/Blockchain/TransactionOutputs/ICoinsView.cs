using System.Diagnostics.CodeAnalysis;
using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.Models;

namespace WabiSabiMonitor.ApplicationCore.Utils.Blockchain.TransactionOutputs;

public interface ICoinsView : IEnumerable<SmartCoin>
{
	ICoinsView AtBlockHeight(Height height);

	ICoinsView Available();

	ICoinsView CoinJoinInProcess();

	ICoinsView Confirmed();

	ICoinsView FilterBy(Func<SmartCoin, bool> expression);

	ICoinsView CreatedBy(uint256 txid);

	ICoinsView SpentBy(uint256 txid);

	SmartCoin[] ToArray();

	Money TotalAmount();

	ICoinsView Unconfirmed();

	ICoinsView Unspent();

	bool TryGetByOutPoint(OutPoint outpoint, [NotNullWhen(true)] out SmartCoin? coin);
}
