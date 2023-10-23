using Newtonsoft.Json;
using WabiSabiMonitor.Utils.Backend.Models;
using WabiSabiMonitor.Utils.Helpers;

namespace WabiSabiMonitor.Utils.JsonConverters;

public class FilterModelJsonConverter : JsonConverter<FilterModel>
{
	/// <inheritdoc />
	public override FilterModel? ReadJson(JsonReader reader, Type objectType, FilterModel? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		var value = Guard.Correct((string?)reader.Value);

		return string.IsNullOrWhiteSpace(value) ? default : FilterModel.FromLine(value);
	}

	/// <inheritdoc />
	public override void WriteJson(JsonWriter writer, FilterModel? value, JsonSerializer serializer)
	{
		var filterModel = value?.ToLine() ?? throw new ArgumentNullException(nameof(value));
		writer.WriteValue(filterModel);
	}
}
