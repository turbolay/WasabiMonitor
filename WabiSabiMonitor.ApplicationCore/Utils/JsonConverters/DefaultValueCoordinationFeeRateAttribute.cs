using System.ComponentModel;
using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Utils.JsonConverters;

public class DefaultValueCoordinationFeeRateAttribute : DefaultValueAttribute
{
	public DefaultValueCoordinationFeeRateAttribute(double feeRate, double plebsDontPayThreshold)
		: base(new CoordinationFeeRate((decimal)feeRate, Money.Coins((decimal)plebsDontPayThreshold)))
	{
	}
}
