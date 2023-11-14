using NBitcoin;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

public record InputsRemovalRequest(
	uint256 RoundId,
	Guid AliceId
);
