using System.Net.Http.Headers;

namespace WabiSabiMonitor.ApplicationCore.Utils.Tor.Http.Models;

public record HttpRequestContentHeaders(
	HttpRequestHeaders RequestHeaders,
	HttpContentHeaders ContentHeaders);
