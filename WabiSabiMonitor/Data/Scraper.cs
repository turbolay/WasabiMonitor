using System.Threading.Channels;
using WabiSabiMonitor.Utils.Bases;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data;

public class Scraper : PeriodicRunner
{
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    public static readonly Channel<Program.PublicStatus> ToBeProcessedData = Channel.CreateUnbounded<Program.PublicStatus>();
    
    protected override async Task ActionAsync(CancellationToken cancel)
    {
        var rounds = await Program.WabiSabiApiClient!.GetStatusAsync(RoundStateRequest.Empty, CancellationToken.None);
        var humanMonitor = await Program.WabiSabiApiClient.GetHumanMonitor(new(), CancellationToken.None);

        var publicStatus = new Program.PublicStatus(DateTimeOffset.UtcNow, rounds, humanMonitor);
        await ToBeProcessedData.Writer.WriteAsync(publicStatus, cancel);
    }
    
    public Scraper() : base(Interval)
    {
    }
}