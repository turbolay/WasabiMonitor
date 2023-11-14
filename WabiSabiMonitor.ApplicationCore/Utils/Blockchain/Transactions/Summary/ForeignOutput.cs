using NBitcoin;

namespace WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Transactions.Summary;

public class ForeignOutput : Output
{
	public ForeignOutput(Money amount, BitcoinAddress destinationAddress) : base(amount, destinationAddress)
	{
	}
}
