using WabiSabiMonitor.ApplicationCore.Utils.Backend.Models.Responses;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client;

public interface IWasabiBackendStatusProvider
{
	SynchronizeResponse? LastResponse { get; }
}
