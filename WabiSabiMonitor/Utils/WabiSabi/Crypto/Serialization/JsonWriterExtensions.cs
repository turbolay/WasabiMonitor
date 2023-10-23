using Newtonsoft.Json;

namespace WabiSabiMonitor.Utils.WabiSabi.Crypto.Serialization;

public static class JsonWriterExtensions
{
	public static void WriteProperty<T>(this JsonWriter writer, string name, T value, JsonSerializer serializer)
	{
		writer.WritePropertyName(name);
		serializer.Serialize(writer, value);
	}
}
