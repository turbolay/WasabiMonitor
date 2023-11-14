using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Utils.JsonConverters;

namespace WabiSabiMonitor.ApplicationCore.Utils.Backend.Models.Responses;

public class FiltersResponse
{
	public int BestHeight { get; set; }

	[JsonProperty(ItemConverterType = typeof(FilterModelJsonConverter))] // Do not use the default jsonifyer, because that's too much data.
	public IEnumerable<FilterModel> Filters { get; set; } = new List<FilterModel>();
}
