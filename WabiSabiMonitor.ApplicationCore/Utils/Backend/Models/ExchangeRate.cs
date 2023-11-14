using System.ComponentModel.DataAnnotations;

namespace WabiSabiMonitor.ApplicationCore.Utils.Backend.Models;

public class ExchangeRate
{
	[Required]
	public string Ticker { get; set; } = "";

	[Required]
	public decimal Rate { get; set; }
}
