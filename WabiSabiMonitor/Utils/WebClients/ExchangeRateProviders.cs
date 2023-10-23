using WabiSabiMonitor.Utils.Backend.Models;
using WabiSabiMonitor.Utils.Interfaces;
using WabiSabiMonitor.Utils.Logging;
using WabiSabiMonitor.Utils.WebClients.Bitstamp;
using WabiSabiMonitor.Utils.WebClients.BlockchainInfo;
using WabiSabiMonitor.Utils.WebClients.Coinbase;
using WabiSabiMonitor.Utils.WebClients.CoinGecko;
using WabiSabiMonitor.Utils.WebClients.Gemini;

namespace WabiSabiMonitor.Utils.WebClients;

public class ExchangeRateProvider : IExchangeRateProvider
{
	private readonly IExchangeRateProvider[] _exchangeRateProviders =
	{
		new BlockchainInfoExchangeRateProvider(),
		new BitstampExchangeRateProvider(),
		new CoinGeckoExchangeRateProvider(),
		new CoinbaseExchangeRateProvider(),
		new GeminiExchangeRateProvider()
	};

	public async Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken)
	{
		foreach (var provider in _exchangeRateProviders)
		{
			try
			{
				return await provider.GetExchangeRateAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				// Ignore it and try with the next one
				Logger.LogTrace(ex);
			}
		}
		return Enumerable.Empty<ExchangeRate>();
	}
}
