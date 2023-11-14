using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Models;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

public record Error(
	string Type,
	string ErrorCode,
	string Description,
	ExceptionData ExceptionData
);
