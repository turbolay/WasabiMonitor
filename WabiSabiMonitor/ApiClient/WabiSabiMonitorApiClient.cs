using WabiSabiMonitor.Utils.Logging;
using WabiSabiMonitor.Utils.Rpc;

namespace WabiSabiMonitor.ApiClient;
public class WabiSabiMonitorApiClient : IJsonRpcService
{
	[JsonRpcMethod("hello-world")]
	// Call this between scraping loops
	public static string HelloWorld()
	{
		var message = "Hello world!";
		Logger.LogInfo(message);
		return message;
	}
}
