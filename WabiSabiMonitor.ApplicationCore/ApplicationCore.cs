using Microsoft.Extensions.DependencyInjection;
using WabiSabiMonitor.ApplicationCore.Data;
using WabiSabiMonitor.ApplicationCore.Utils.Logging;

namespace WabiSabiMonitor.ApplicationCore
{
    public class ApplicationCore
    {
        public ApplicationCore()
        {

        }

        public async Task Run()
        {
            var roundStatusScraper = Di.ServiceProvider.GetRequiredService<Scraper>();
            var dataProcessor = Di.ServiceProvider.GetRequiredService<RoundDataReaderService>();
            var rpcServerController = Di.ServiceProvider.GetRequiredService<RpcServerController>();

            Logger.InitializeDefaults("./logs.txt");
            Logger.LogInfo("Read confi...");

            await roundStatusScraper.StartAsync(CancellationTokenSource.Token);
            await roundStatusScraper.TriggerAndWaitRoundAsync(CancellationTokenSource.Token);

            await Scraper.ToBeProcessedData.Reader.WaitToReadAsync(CancellationTokenSource.Token);

            var dataProcessor = host.Services.GetRequiredService<RoundDataReaderService>();
            dataProcessor = new(repository.ReadFromFileSystem());
            await dataProcessor.StartAsync(CancellationTokenSource.Token);

            await rpcServerController.StartRpcServerAsync(CancellationTokenSource.Token);
            Logger.LogInfo("Initialized.");
            await Task.Delay(-1, CancellationTokenSource.Token);
        }
    }
}