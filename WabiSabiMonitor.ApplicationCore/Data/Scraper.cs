using System.Threading.Channels;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Bases;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.PostRequests;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class Scraper : PeriodicRunner
{
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    public readonly Channel<PublicStatus> ToBeProcessedData = Channel.CreateUnbounded<PublicStatus>();

    private readonly IWabiSabiApiRequestHandler _wabiSabiHttpApiClient;
    private readonly IWabiSabiApiRequestHandlerAdapter _adapter;

    public Scraper(IWabiSabiApiRequestHandler wabiSabiApiRequestHandler, IWabiSabiApiRequestHandlerAdapter wabiSabiApiRequestHandlerAdapter) : base(Interval)
    {
        _wabiSabiHttpApiClient = wabiSabiApiRequestHandler;
        _adapter = wabiSabiApiRequestHandlerAdapter;
    }
    
    protected override async Task ActionAsync(CancellationToken token)
    {

        var status = await _wabiSabiHttpApiClient!.GetStatusAsync(RoundStateRequest.Empty, token);
        Logger.LogInfo($"Total received rounds state: {status.RoundStates.Length}");

        var humanMonitor = await _adapter.GetHumanMonitor(new(), CancellationToken.None);
        var publicStatus = new PublicStatus(DateTimeOffset.UtcNow, status, humanMonitor);
        
        await ToBeProcessedData.Writer.WriteAsync(publicStatus, token);
    }
}