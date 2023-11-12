using System.Threading.Channels;
using WabiSabiMonitor.Utils.Bases;
using WabiSabiMonitor.Utils.WabiSabi.Client;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data;

public class Scraper : PeriodicRunner
{
    private WabiSabiHttpApiClient _wabiSabiHttpApiClient;
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    public static readonly Channel<Program.PublicStatus> ToBeProcessedData = Channel.CreateUnbounded<Program.PublicStatus>();
    
    public Scraper(WabiSabiHttpApiClient WabiSabiHttpApiClient) : base(Interval)
    {
        _wabiSabiHttpApiClient = WabiSabiHttpApiClient;
    }
    
    protected override async Task ActionAsync(CancellationToken cancel)
    {
        var rounds = await _wabiSabiHttpApiClient!.GetStatusAsync(RoundStateRequest.Empty, CancellationToken.None);
        var humanMonitor = await _wabiSabiHttpApiClient.GetHumanMonitor(new(), CancellationToken.None);

        var publicStatus = new Program.PublicStatus(DateTimeOffset.UtcNow, rounds, humanMonitor);
        await ToBeProcessedData.Writer.WriteAsync(publicStatus, cancel);
    }
    
   
}