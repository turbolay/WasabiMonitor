using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Models;

public record WrongPhaseExceptionData(Phase CurrentPhase) : ExceptionData;
