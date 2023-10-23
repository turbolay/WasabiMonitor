using Newtonsoft.Json;
using WabiSabiMonitor.Utils.JsonConverters;

namespace WabiSabiMonitor.Utils.Backend.Models.Responses;

public class FiltersResponse
{
	public int BestHeight { get; set; }

	[JsonProperty(ItemConverterType = typeof(FilterModelJsonConverter))] // Do not use the default jsonifyer, because that's too much data.
	public IEnumerable<FilterModel> Filters { get; set; } = new List<FilterModel>();
}
