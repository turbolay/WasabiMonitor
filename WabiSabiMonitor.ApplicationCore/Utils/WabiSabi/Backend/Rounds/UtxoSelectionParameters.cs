using System.Collections.Immutable;
using NBitcoin;
using WabiSabi.Crypto.Randomness;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;

public record UtxoSelectionParameters(
	MoneyRange AllowedInputAmounts,
	MoneyRange AllowedOutputAmounts,
	CoordinationFeeRate CoordinationFeeRate,
	FeeRate MiningFeeRate,
	ImmutableSortedSet<ScriptType> AllowedInputScriptTypes)
{
	public static UtxoSelectionParameters FromRoundParameters(RoundParameters roundParameters, WasabiRandom? random = null)
	{
		random ??= SecureRandom.Instance;

		return new(
			roundParameters.AllowedInputAmounts,
			roundParameters.CalculateReasonableOutputAmountRange(random),
			roundParameters.CoordinationFeeRate,
			roundParameters.MiningFeeRate,
			roundParameters.AllowedInputTypes);
	}
}
