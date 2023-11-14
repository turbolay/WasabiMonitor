using WabiSabi.CredentialRequesting;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

public record ConnectionConfirmationResponse(
	CredentialsResponse ZeroAmountCredentials,
	CredentialsResponse ZeroVsizeCredentials,
	CredentialsResponse? RealAmountCredentials = null,
	CredentialsResponse? RealVsizeCredentials = null
);
