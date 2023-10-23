using Newtonsoft.Json;

namespace WabiSabiMonitor.Utils.WebClients.Bitstamp;

public class BitstampExchangeRate
{
	[JsonProperty(PropertyName = "bid")]
	public decimal Rate { get; set; }
}
