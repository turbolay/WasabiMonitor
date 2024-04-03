using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Utils.JsonConverters;
using WabiSabiMonitor.ApplicationCore.Utils.JsonConverters.Bitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.JsonConverters.Timing;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Crypto.Serialization;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;

public class JsonSerializationOptions
{
    public static readonly JsonSerializerSettings CurrentSettings = new()
    {
        Converters = new List<JsonConverter>()
            {
                new ScalarJsonConverter(),
                new GroupElementJsonConverter(),
                new OutPointJsonConverter(),
                new WitScriptJsonConverter(),
                new ScriptJsonConverter(),
                new OwnershipProofJsonConverter(),
                new NetworkJsonConverter(),
                new FeeRateJsonConverter(),
                new MoneySatoshiJsonConverter(),
                new Uint256JsonConverter(),
                new Uint256DictionaryJsonConverter(),
                new MultipartyTransactionStateJsonConverter(),
                new ExceptionDataJsonConverter(),
                new ExtPubKeyJsonConverter(),
                new TimeSpanJsonConverter(),
                new CoinJsonConverter(),
                new CoinJoinEventJsonConverter(),
                new GroupElementVectorJsonConverter(),
                new ScalarVectorJsonConverter(),
                new IssuanceRequestJsonConverter(),
                new CredentialPresentationJsonConverter(),
                new ProofJsonConverter(),
                new MacJsonConverter()
            },
        Formatting = Formatting.Indented
    };

    public static readonly JsonSerializationOptions Default = new();

    private JsonSerializationOptions()
    {
    }

    public JsonSerializerSettings Settings => CurrentSettings;
}