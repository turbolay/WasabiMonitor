using NBitcoin;

namespace WabiSabiMonitor.Utils.WabiSabi.Models;

public record InputsRemovalRequest(
	uint256 RoundId,
	Guid AliceId
);
