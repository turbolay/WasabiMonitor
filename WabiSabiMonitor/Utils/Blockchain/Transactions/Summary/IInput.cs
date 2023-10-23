using NBitcoin;

namespace WabiSabiMonitor.Utils.Blockchain.Transactions.Summary;

public interface IInput
{
	Money? Amount { get; }
}
