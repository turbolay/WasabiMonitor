using Newtonsoft.Json;

namespace WabiSabiMonitor.ApplicationCore.Utils.WebClients.Bitstamp;

public class BitstampExchangeRate
{
	[JsonProperty(PropertyName = "bid")]
	public decimal Rate { get; set; }
}
