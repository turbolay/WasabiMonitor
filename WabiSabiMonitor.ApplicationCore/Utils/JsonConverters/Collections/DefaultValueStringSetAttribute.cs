using System.ComponentModel;
using Newtonsoft.Json;

namespace WabiSabiMonitor.ApplicationCore.Utils.JsonConverters.Collections;

public class DefaultValueStringSetAttribute : DefaultValueAttribute
{
	public DefaultValueStringSetAttribute(string json)
		: base(JsonConvert.DeserializeObject<ISet<string>>(json))
	{
	}
}
