using NBitcoin;

namespace WabiSabiMonitor.Utils.Blockchain.Transactions.Summary;

public class KnownInput : IInput
{
	public KnownInput(Money amount)
	{
		Amount = amount;
	}

	public virtual Money? Amount { get; }
}
