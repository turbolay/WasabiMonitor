using WabiSabiMonitor.ApplicationCore.Utils.Backend.Models;

namespace WabiSabiMonitor.ApplicationCore.Utils.Interfaces;

public interface IExchangeRateProvider
{
	Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken);
}
