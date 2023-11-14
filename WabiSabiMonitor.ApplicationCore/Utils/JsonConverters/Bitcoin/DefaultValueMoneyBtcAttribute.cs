using System.ComponentModel;

namespace WabiSabiMonitor.ApplicationCore.Utils.JsonConverters.Bitcoin;

public class DefaultValueMoneyBtcAttribute : DefaultValueAttribute
{
	public DefaultValueMoneyBtcAttribute(string json) : base(MoneyBtcJsonConverter.Parse(json))
	{
	}
}
