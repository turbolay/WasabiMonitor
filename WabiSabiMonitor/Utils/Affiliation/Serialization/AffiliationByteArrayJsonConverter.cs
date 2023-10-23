using Newtonsoft.Json;
using WabiSabiMonitor.Utils.Helpers;

namespace WabiSabiMonitor.Utils.Affiliation.Serialization;

public class AffiliationByteArrayJsonConverter : JsonConverter<byte[]>
{
	public override byte[]? ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.Value is string serialized)
		{
			return Convert.FromHexString(serialized);
		}

		throw new JsonSerializationException("Cannot deserialize object.");
	}

	public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
	{
		Guard.NotNull(nameof(value), value);
		writer.WriteValue(Convert.ToHexString(value).ToLower());
	}
}
