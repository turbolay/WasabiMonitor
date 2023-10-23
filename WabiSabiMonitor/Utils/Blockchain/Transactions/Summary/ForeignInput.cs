using NBitcoin;

namespace WabiSabiMonitor.Utils.Blockchain.Transactions.Summary;

public class ForeignInput : IInput
{
	public Money? Amount => default;
}
