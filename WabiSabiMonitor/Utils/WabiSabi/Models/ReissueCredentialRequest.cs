using NBitcoin;
using WabiSabi.CredentialRequesting;

namespace WabiSabiMonitor.Utils.WabiSabi.Models;

public record ReissueCredentialRequest(
	uint256 RoundId,
	RealCredentialsRequest RealAmountCredentialRequests,
	RealCredentialsRequest RealVsizeCredentialRequests,
	ZeroCredentialsRequest ZeroAmountCredentialRequests,
	ZeroCredentialsRequest ZeroVsizeCredentialsRequests
);
