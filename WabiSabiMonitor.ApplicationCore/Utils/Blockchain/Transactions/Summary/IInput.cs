using NBitcoin;

namespace WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Transactions.Summary;

public interface IInput
{
	Money? Amount { get; }
}
