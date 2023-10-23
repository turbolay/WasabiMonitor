using Microsoft.Extensions.Hosting;
using WabiSabiMonitor.Utils.Helpers;

namespace WabiSabiMonitor.Utils.Services;

public class HostedService
{
	public HostedService(IHostedService service, string friendlyName)
	{
		Service = Guard.NotNull(nameof(service), service);
		FriendlyName = Guard.NotNull(nameof(friendlyName), friendlyName);
	}

	public IHostedService Service { get; }
	public string FriendlyName { get; }
}
