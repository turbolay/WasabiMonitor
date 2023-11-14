using Newtonsoft.Json;

namespace WabiSabiMonitor.ApplicationCore.Utils.WebClients.CoinGecko;

public class CoinGeckoExchangeRate
{
	[JsonProperty(PropertyName = "current_price")]
	public decimal Rate { get; set; }
}
