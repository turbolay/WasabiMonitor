using WabiSabiMonitor.Utils.WabiSabi.Backend.Models;

namespace WabiSabiMonitor.Utils.WabiSabi.Models;

public record Error(
	string Type,
	string ErrorCode,
	string Description,
	ExceptionData ExceptionData
);
