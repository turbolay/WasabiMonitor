using WabiSabiMonitor.Utils.WabiSabi.Client.StatusChangedEvents;

namespace WabiSabiMonitor.Utils.Exceptions;

public class CoinJoinClientException : Exception
{
	public CoinJoinClientException(CoinjoinError coinjoinError, string? message = null) : base($"Coinjoin aborted with error: {coinjoinError}. {message}")
	{
		CoinjoinError = coinjoinError;
	}

	public CoinjoinError CoinjoinError { get; }
}
