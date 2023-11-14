using System.Net.Http.Headers;

namespace WabiSabiMonitor.ApplicationCore.Utils.Tor.Http.Models;

public record HttpResponseContentHeaders(
	HttpResponseHeaders ResponseHeaders,
	HttpContentHeaders ContentHeaders);
