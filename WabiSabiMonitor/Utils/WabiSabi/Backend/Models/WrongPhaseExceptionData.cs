using WabiSabiMonitor.Utils.WabiSabi.Backend.Rounds;

namespace WabiSabiMonitor.Utils.WabiSabi.Backend.Models;

public record WrongPhaseExceptionData(Phase CurrentPhase) : ExceptionData;
