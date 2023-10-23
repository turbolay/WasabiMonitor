using System.Net.Http.Headers;

namespace WabiSabiMonitor.Utils.Tor.Http.Models;

public record HttpRequestContentHeaders(
	HttpRequestHeaders RequestHeaders,
	HttpContentHeaders ContentHeaders);
