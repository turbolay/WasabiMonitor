using System.Net.Http.Headers;
using WabiSabiMonitor.Utils.Backend.Models;
using WabiSabiMonitor.Utils.Helpers;
using WabiSabiMonitor.Utils.Interfaces;
using WabiSabiMonitor.Utils.Tor.Http.Extensions;

namespace WabiSabiMonitor.Utils.WebClients.CoinGecko;

public class CoinGeckoExchangeRateProvider : IExchangeRateProvider
{
	public async Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken)
	{
		using var httpClient = new HttpClient
		{
			BaseAddress = new Uri("https://api.coingecko.com")
		};
		httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WasabiWallet", Constants.ClientVersion.ToString()));
		using var response = await httpClient.GetAsync("api/v3/coins/markets?vs_currency=usd&ids=bitcoin", cancellationToken).ConfigureAwait(false);
		using var content = response.Content;
		var rates = await content.ReadAsJsonAsync<CoinGeckoExchangeRate[]>().ConfigureAwait(false);

		var exchangeRates = new List<ExchangeRate>
			{
				new ExchangeRate { Rate = rates[0].Rate, Ticker = "USD" }
			};

		return exchangeRates;
	}
}
