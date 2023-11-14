using System.Net;
using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.Helpers;

namespace WabiSabiMonitor.ApplicationCore.Utils.Models;

public class ServiceConfiguration
{
	public ServiceConfiguration(
		EndPoint bitcoinCoreEndPoint,
		Money dustThreshold)
	{
		BitcoinCoreEndPoint = Guard.NotNull(nameof(bitcoinCoreEndPoint), bitcoinCoreEndPoint);
		DustThreshold = Guard.NotNull(nameof(dustThreshold), dustThreshold);
	}

	public EndPoint BitcoinCoreEndPoint { get; set; }
	public Money DustThreshold { get; set; }
}
