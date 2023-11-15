using Microsoft.Extensions.DependencyInjection;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;

namespace WabiSabiMonitor.ApplicationCore
{
    public class ApplicationCore
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Scraper _roundStatusScraper;
        private readonly RoundDataReaderService _dataProcessor;
        private readonly RpcServerController _rpcServerController;

        public ApplicationCore(Scraper roundStatusScraper, RoundDataReaderService dataProcessor,
            RpcServerController rpcServerController)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _roundStatusScraper = roundStatusScraper;
            _dataProcessor = dataProcessor;
            _rpcServerController = rpcServerController;
        }

        public async Task Run()
        {
            Logger.InitializeDefaults("./logs.txt");
            Logger.LogInfo("Read confi...");

            //changed to _cancellationTokenSource from CancellationTokenSource.Token
            await _roundStatusScraper.StartAsync(_cancellationTokenSource.Token);
            await _roundStatusScraper.TriggerAndWaitRoundAsync(_cancellationTokenSource.Token);

            await Scraper.ToBeProcessedData.Reader.WaitToReadAsync(_cancellationTokenSource.Token);

            await _dataProcessor.StartAsync(_cancellationTokenSource.Token);

            await _rpcServerController.StartRpcServerAsync(_cancellationTokenSource.Token);
            Logger.LogInfo("Initialized.");
            await Task.Delay(-1, _cancellationTokenSource.Token);
        }
    }
}