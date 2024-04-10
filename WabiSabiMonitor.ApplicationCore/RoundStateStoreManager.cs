using Microsoft.Extensions.Hosting;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Bases;
using WabiSabiMonitor.ApplicationCore.Utils.Helpers;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;

namespace WabiSabiMonitor.ApplicationCore
{
    public class RoundStateStoreManager : BackgroundService
    {
        private readonly IRoundDataReaderService _roundDataReaderService;

        public RoundStateStoreManager(IRoundDataReaderService roundDataReaderService)
        {
            _roundDataReaderService = roundDataReaderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<KeyValuePair<uint256, RoundDataReaderService.ProcessedRound>> savedForNextDay = new();
            while (!stoppingToken.IsCancellationRequested)
            {
                var date = DateTime.Now.Date;
                await WaitUntilMidnightAsync(stoppingToken);
                Logger.LogInfo("It's midnight. Writing Daily Round States...");

                Dictionary<uint256, RoundDataReaderService.ProcessedRound> allRounds = _roundDataReaderService.Rounds;

                // Get rounds that happened today.
                var roundsToday = allRounds.Where(x => x.Value.LastUpdate.Date == date).ToList();
                roundsToday.AddRange(savedForNextDay);
                savedForNextDay.Clear();

                // Save finished rounds.
                var finishedRounds = roundsToday.Where(x => x.Value.Round.EndRoundState != EndRoundState.None);

                // If a BlameRound is still active, don't save its BlameOf.
                var stillActiveBlameRoundsBlameOfIds = roundsToday.Where(x => x.Value.Round.EndRoundState == EndRoundState.None && x.Value.Round.IsBlame())
                    .Select(x => x.Value.Round.BlameOf);

                var dataToWrite = finishedRounds.Where(round =>
                {
                    bool isBlameOf = stillActiveBlameRoundsBlameOfIds.Contains(round.Key);
                    if (isBlameOf)
                    {
                        Logger.LogInfo($"Excluded Round {round.Key}, is BlameOf.");
                        savedForNextDay.Add(round);
                    }

                    return !isBlameOf;
                });

                var path = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "Client")), $"RoundStates_{date:yyyy-MM-dd}.json");
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dataToWrite, JsonSerializationOptions.CurrentSettings), stoppingToken);
            }
        }

        private async Task WaitUntilMidnightAsync(CancellationToken stoppingToken)
        {
            var now = DateTime.Now.TimeOfDay;
            TimeSpan midnight = new(0, 0, 0);

            if (now > midnight)
            {
                midnight = midnight.Add(TimeSpan.FromDays(1));
            }

            var waitUntil = midnight - now;

            await Task.Delay(waitUntil, stoppingToken);
        }
    }
}