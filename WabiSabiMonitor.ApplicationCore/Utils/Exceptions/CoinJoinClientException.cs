using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client.StatusChangedEvents;

namespace WabiSabiMonitor.ApplicationCore.Utils.Exceptions;

public class CoinJoinClientException : Exception
{
	public CoinJoinClientException(CoinjoinError coinjoinError, string? message = null) : base($"Coinjoin aborted with error: {coinjoinError}. {message}")
	{
		CoinjoinError = coinjoinError;
	}

	public CoinjoinError CoinjoinError { get; }
}
