using NBitcoin;

namespace WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Transactions.Summary;

public class ForeignInput : IInput
{
	public Money? Amount => default;
}
