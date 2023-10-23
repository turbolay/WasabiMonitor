using NBitcoin;

namespace WabiSabiMonitor.Utils.Blockchain.Transactions.Summary;

public class OwnOutput : Output
{
	public OwnOutput(Money amount, BitcoinAddress destinationAddress, bool isInternal) : base(amount, destinationAddress)
	{
		IsInternal = isInternal;
	}

	public bool IsInternal { get; }
}
