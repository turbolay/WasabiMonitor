using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WabiSabiMonitor.ApplicationCore.Data;

namespace WabiSabiMonitor.ApplicationCore.Utils.JsonConverters;

public class Uint256DictionaryJsonConverter : JsonConverter<Dictionary<uint256, RoundDataReaderService.ProcessedRound>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<uint256, RoundDataReaderService.ProcessedRound>? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var jObject = new JObject();

        foreach (var kvp in value)
        {
            string key = kvp.Key.ToString() ?? string.Empty;
            JToken tokenValue = JToken.FromObject(kvp.Value, serializer);

            jObject.Add(key, tokenValue);
        }

        // Write the JObject using the JsonWriter
        jObject.WriteTo(writer);
    }


    public override Dictionary<uint256, RoundDataReaderService.ProcessedRound>? ReadJson(JsonReader reader, Type objectType, Dictionary<uint256, RoundDataReaderService.ProcessedRound>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var result = new Dictionary<uint256, RoundDataReaderService.ProcessedRound?>();
        var jsonObject = JObject.Load(reader);

        foreach (var property in jsonObject.Properties())
        {
            // Use your Uint256JsonConverter to convert the property name into a uint256
            uint256? key = new Uint256JsonConverter().ReadJson(property.Name, typeof(uint256), existingValue: null, hasExistingValue: false, serializer);

            // Deserialize the value part of the property
            RoundDataReaderService.ProcessedRound? value = property.Value.ToObject<RoundDataReaderService.ProcessedRound>(serializer);

            if (key != null) result[key] = value;
        }

        return result!;
    }
}