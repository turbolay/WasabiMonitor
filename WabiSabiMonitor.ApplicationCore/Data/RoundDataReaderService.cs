using Microsoft.Extensions.Hosting;
using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Affiliation.Models;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Data;

public class RoundDataReaderService : BackgroundService, IRoundDataReaderService
{
    private readonly Scraper _scraper;
    public Dictionary<uint256, ProcessedRound> Rounds { get; }
    public HumanMonitorResponse? LastHumanMonitor { get; private set; }

    public RoundDataReaderService(Dictionary<uint256, ProcessedRound> savedRounds, Scraper scraper)
    {
        Rounds = savedRounds;
        _scraper = scraper;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (await _scraper.ToBeProcessedData.Reader.WaitToReadAsync(token) || !token.IsCancellationRequested)
        {
            var data = await _scraper.ToBeProcessedData.Reader.ReadAsync(token);

            foreach (var round in data.Rounds.RoundStates)
            {
                if (!Rounds.TryGetValue(round.Id, out var oldInstance))
                {
                    Rounds.Add(
                        round.Id,
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