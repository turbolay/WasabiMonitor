using Microsoft.Extensions.Hosting;
using NBitcoin;
using Newtonsoft.Json;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Interfaces;
using WabiSabiMonitor.ApplicationCore.Utils.Helpers;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;

namespace WabiSabiMonitor.ApplicationCore
{
    public class RoundStateStoreManager : BackgroundService
    {
        private readonly IRoundsDataFilter _roundsDataFilter;

        public RoundStateStoreManager(IRoundsDataFilter roundsDataFilter)
        {
            _roundsDataFilter = roundsDataFilter;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<RoundState> savedForNextDay = new();
            while (!stoppingToken.IsCancellationRequested)
            {
                var date = DateTime.UtcNow.Date;
                await WaitUntilMidnightAsync(stoppingToken);
                Logger.LogInfo("It's midnight. Writing Daily Round States...");

                // Get rounds that happened today.
                var roundsToday = _roundsDataFilter.GetRoundsInInterval(date, date.AddDays(1)).ToList();

                // Add leftovers from yesterday.
                roundsToday.AddRange(savedForNextDay);
                savedForNextDay.Clear();

                // Save finished rounds.
                var finishedRounds = _roundsDataFilter.GetRoundsFinishedSince(date).ToArray();

                // If a BlameRound is still active, don't save its BlameOf.
                var stillActiveBlameRoundsBlameOfIds = roundsToday.Where(x => x.EndRoundState == EndRoundState.None && x.IsBlame())
                    .Select(x => x.BlameOf).ToArray();

                var dataToWrite = finishedRounds.Where(round =>
                {
                    bool isBlameOf = stillActiveBlameRoundsBlameOfIds.Contains(round.Id);
                    if (isBlameOf)
                    {
                        Logger.LogInfo($"Excluded Round {round.Id}, is BlameOf.");
                        savedForNextDay.Add(round);
                    }

                    return !isBlameOf;
                }).ToArray();

                var path = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("WabiSabiMonitor", "DataStore", "RoundState")), $"RoundStates_{date:yyyy-MM-dd}.json");
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dataToWrite, JsonSerializationOptions.CurrentSettings), stoppingToken);
            }
        }

        private async Task WaitUntilMidnightAsync(CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow.TimeOfDay;
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