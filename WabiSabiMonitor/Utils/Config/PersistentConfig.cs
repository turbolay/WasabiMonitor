using System.ComponentModel;
using Newtonsoft.Json;

namespace WabiSabiMonitor.Utils.Config;

[JsonObject(MemberSerialization.OptIn)]
public class PersistentConfig : ConfigBase
{
	public const int DefaultJsonRpcServerPort = 37127;

	/// <summary>
	/// Constructor for config population using Newtonsoft.JSON.
	/// </summary>
	public PersistentConfig() : base()
	{
	}

	public PersistentConfig(string filePath) : base(filePath)
	{
	}

	[DefaultValue("")]
	[JsonProperty(PropertyName = "JsonRpcUser", DefaultValueHandling = DefaultValueHandling.Populate)]
	public string JsonRpcUser { get; internal set; } = "";

	[DefaultValue("")]
	[JsonProperty(PropertyName = "JsonRpcPassword", DefaultValueHandling = DefaultValueHandling.Populate)]
	public string JsonRpcPassword { get; internal set; } = "";

	[JsonProperty(PropertyName = "JsonRpcServerPrefixes")]
	public string[] JsonRpcServerPrefixes { get; internal set; } = new[]
	{
		"http://127.0.0.1:37127/",
		"http://localhost:37127/"
	};
	
}
