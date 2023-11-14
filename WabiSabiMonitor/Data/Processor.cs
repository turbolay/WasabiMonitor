using Microsoft.Extensions.Hosting;
using NBitcoin;
using WabiSabiMonitor.Utils.Affiliation.Models;
using WabiSabiMonitor.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Data;

public class Processor : BackgroundService
{
    public Dictionary<uint256, ProcessedRound> Rounds { get; }
    public HumanMonitorResponse? LastHumanMonitor { get; private set; }

    public Processor(Dictionary<uint256, ProcessedRound> savedRounds)
    {
        Rounds = savedRounds;
    }
    
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (await Scraper.ToBeProcessedData.Reader.WaitToReadAsync(token) || !token.IsCancellationRequested)
        {
            var data = await Scraper.ToBeProcessedData.Reader.ReadAsync(token);

            foreach (var round in data.Rounds.RoundStates)
            {
                if (!Rounds.TryGetValue(round.Id, out var oldInstance))
                {
                    Rounds.Add(round.Id,
                        new ProcessedRound(
                            data.ScrapedAt, 
                            round, 
                            data.Rounds.AffiliateInformation, 
                            data.Rounds.CoinJoinFeeRateMedians));
                    continue;
                }

                // Do not update finished round.
                if (oldInstance.Round.EndRoundState != EndRoundState.None)
                {
                    continue;
                }

                oldInstance.Round = round;
                oldInstance.LastUpdate = data.ScrapedAt;
            }

            LastHumanMonitor = data.HumanMonitor;
        }
    }
    
    public record ProcessedRound(DateTimeOffset LastUpdate, RoundState Round, AffiliateInformation Affiliates,
        CoinJoinFeeRateMedian[] CoinJoinFeeRateMedian)
    {
        public DateTimeOffset LastUpdate { get; set; } = LastUpdate;
        public RoundState Round { get; set; } = Round;
    }
}