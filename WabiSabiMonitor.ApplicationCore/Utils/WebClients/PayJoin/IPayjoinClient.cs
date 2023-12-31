using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Keys;

namespace WabiSabiMonitor.ApplicationCore.Utils.WebClients.PayJoin;

public interface IPayjoinClient
{
	Uri PaymentUrl { get; }

	Task<PSBT> RequestPayjoin(PSBT originalTx, IHDKey accountKey, RootedKeyPath rootedKeyPath, HdPubKey changeHdPubKey, CancellationToken cancellationToken);
}
