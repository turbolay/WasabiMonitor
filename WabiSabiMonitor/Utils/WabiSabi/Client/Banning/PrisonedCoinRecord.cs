using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.Utils.JsonConverters;

namespace WabiSabiMonitor.Utils.WabiSabi.Client.Banning;

public record PrisonedCoinRecord
{
	public PrisonedCoinRecord(OutPoint outpoint, DateTimeOffset bannedUntil)
	{
		Outpoint = outpoint;
		BannedUntil = bannedUntil;
	}

	[JsonProperty]
	[JsonConverter(typeof(OutPointJsonConverter))]
	public OutPoint Outpoint { get; set; }

	public DateTimeOffset BannedUntil { get; set; }
}
