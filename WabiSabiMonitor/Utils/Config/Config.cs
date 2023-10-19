using WabiSabiMonitor.Utils.Helpers;

namespace WabiSabiMonitor.Utils.Config;

public class Config
{
	public PersistentConfig PersistentConfig { get; }
	private string[] Args { get; }

	public Config(PersistentConfig persistentConfig, string[] args)
	{
		PersistentConfig = persistentConfig;
		Args = args;
	}
	
	public string JsonRpcUser => GetEffectiveString(PersistentConfig.JsonRpcUser, key: "JsonRpcUser");
	public string JsonRpcPassword => GetEffectiveString(PersistentConfig.JsonRpcPassword, key: "JsonRpcPassword");
	public string[] JsonRpcServerPrefixes => GetEffectiveValue(PersistentConfig.JsonRpcServerPrefixes, x => new[] { x }, key: "JsonRpcServerPrefixes");
	
	private bool GetEffectiveBool(bool valueInConfigFile, string key) =>
		GetEffectiveValue(
			valueInConfigFile,
			x =>
				bool.TryParse(x, out var value)
				? value
				: throw new ArgumentException("must be 'true' or 'false'.", key),
			Args,
			key);

	private string GetEffectiveString(string valueInConfigFile, string key) =>
		GetEffectiveValue(valueInConfigFile, x => x, Args, key);

	private string? GetEffectiveOptionalString(string? valueInConfigFile, string key) =>
		GetEffectiveValue(valueInConfigFile, x => x, Args, key);

	private T GetEffectiveValue<T>(T valueInConfigFile, Func<string, T> converter, string key) =>
		GetEffectiveValue(valueInConfigFile, converter, Args, key);

	private static string GetString(string valueInConfigFile, string[] args, string key) =>
		GetEffectiveValue(valueInConfigFile, x => x, args, key);
	
	public static string DataDir => GetString(
		EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")),
		Environment.GetCommandLineArgs(),
		"datadir");

	private static T GetEffectiveValue<T>(T valueInConfigFile, Func<string, T> converter, string[] args, string key)
	{
		if (ArgumentHelpers.TryGetValue(key, args, converter, out var cliArg))
		{
			return cliArg;
		}

		var envKey = "WABISABIMONITOR-" + key.ToUpperInvariant();
		var environmentVariables = Environment.GetEnvironmentVariables();
		if (environmentVariables.Contains(envKey))
		{
			if (environmentVariables[envKey] is string envVar)
			{
				return converter(envVar);
			}
		}

		return valueInConfigFile;
	}
}
