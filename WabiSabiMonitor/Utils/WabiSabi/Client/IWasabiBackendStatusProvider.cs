using WabiSabiMonitor.Utils.Backend.Models.Responses;

namespace WabiSabiMonitor.Utils.WabiSabi.Client;

public interface IWasabiBackendStatusProvider
{
	SynchronizeResponse? LastResponse { get; }
}
