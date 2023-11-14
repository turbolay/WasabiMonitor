using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Utils.JsonConverters;
using WabiSabiMonitor.ApplicationCore.Utils.Models;

namespace WabiSabiMonitor.ApplicationCore.Utils.Blockchain.Keys;

[JsonObject(MemberSerialization.OptIn)]
public class BlockchainState
{
	[JsonConstructor]
	public BlockchainState(Network network, Height height, Height turboSyncHeight)
	{
		Network = network;
		Height = height;
		TurboSyncHeight = turboSyncHeight;
	}

	public BlockchainState()
	{
		Network = Network.Main;
		Height = 0;
		TurboSyncHeight = 0;
	}

	public BlockchainState(Network network) : this(network, height: 0, turboSyncHeight: 0)
	{
	}

	[JsonProperty]
	[JsonConverter(typeof(NetworkJsonConverter))]
	public Network Network { get; set; }

	[JsonProperty]
	[JsonConverter(typeof(WalletHeightJsonConverter))]
	public Height Height { get; set; }

	[JsonProperty]
	[JsonConverter(typeof(WalletHeightJsonConverter))]
	public Height TurboSyncHeight { get; set; }
}
