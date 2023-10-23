using NBitcoin;
using WabiSabi.CredentialRequesting;
using WabiSabiMonitor.Utils.Crypto;

namespace WabiSabiMonitor.Utils.WabiSabi.Models;

public record InputRegistrationRequest(
	uint256 RoundId,
	OutPoint Input,
	OwnershipProof OwnershipProof,
	ZeroCredentialsRequest ZeroAmountCredentialRequests,
	ZeroCredentialsRequest ZeroVsizeCredentialRequests
);
