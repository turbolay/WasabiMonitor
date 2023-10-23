using WabiSabiMonitor.Utils.Backend.Models;

namespace WabiSabiMonitor.Utils.Interfaces;

public interface IExchangeRateProvider
{
	Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken);
}
