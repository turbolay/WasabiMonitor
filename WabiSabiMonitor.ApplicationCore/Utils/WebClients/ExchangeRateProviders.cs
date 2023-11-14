using WabiSabiMonitor.ApplicationCore.Utils.Backend.Models;
using WabiSabiMonitor.ApplicationCore.Utils.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.WebClients.Bitstamp;
using WabiSabiMonitor.ApplicationCore.Utils.WebClients.BlockchainInfo;
using WabiSabiMonitor.ApplicationCore.Utils.WebClients.Coinbase;
using WabiSabiMonitor.ApplicationCore.Utils.WebClients.CoinGecko;
using WabiSabiMonitor.ApplicationCore.Utils.WebClients.Gemini;

namespace WabiSabiMonitor.ApplicationCore.Utils.WebClients;

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
