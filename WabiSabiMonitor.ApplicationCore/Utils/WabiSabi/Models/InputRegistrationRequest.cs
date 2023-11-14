using NBitcoin;
using WabiSabi.CredentialRequesting;
using WabiSabiMonitor.ApplicationCore.Utils.Crypto;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

public record InputRegistrationRequest(
	uint256 RoundId,
	OutPoint Input,
	OwnershipProof OwnershipProof,
	ZeroCredentialsRequest ZeroAmountCredentialRequests,
	ZeroCredentialsRequest ZeroVsizeCredentialRequests
);
