using System.ComponentModel;

namespace WabiSabiMonitor.Utils.JsonConverters.Bitcoin;

public class DefaultValueMoneyBtcAttribute : DefaultValueAttribute
{
	public DefaultValueMoneyBtcAttribute(string json) : base(MoneyBtcJsonConverter.Parse(json))
	{
	}
}
