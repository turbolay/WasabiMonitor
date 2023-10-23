using System.ComponentModel;
using WabiSabiMonitor.Utils.Bases;
using WabiSabiMonitor.Utils.Models;
using WabiSabiMonitor.Utils.WebClients.Wasabi;

namespace WabiSabiMonitor.Utils.Services;

public class UpdateChecker : PeriodicRunner
{
	public UpdateChecker(TimeSpan period, WasabiSynchronizer synchronizer) : base(period)
	{
		Synchronizer = synchronizer;
		UpdateStatus = new UpdateStatus(true, true, new Version(), 0, new Version());
		WasabiClient = Synchronizer.HttpClientFactory.SharedWasabiClient;
		Synchronizer.PropertyChanged += Synchronizer_PropertyChanged;
	}

	public event EventHandler<UpdateStatus>? UpdateStatusChanged;

	private WasabiSynchronizer Synchronizer { get; }
	public UpdateStatus UpdateStatus { get; private set; }
	public WasabiClient WasabiClient { get; }

	private void Synchronizer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(WasabiSynchronizer.BackendStatus) &&
			Synchronizer.BackendStatus == BackendStatus.Connected)
		{
			// Any time when the synchronizer detects the backend, we immediately check the versions. GUI relies on UpdateStatus changes.
			TriggerRound();
		}
	}

	protected override async Task ActionAsync(CancellationToken cancel)
	{
		var newUpdateStatus = await WasabiClient.CheckUpdatesAsync(cancel).ConfigureAwait(false);
		if (newUpdateStatus != UpdateStatus)
		{
			UpdateStatus = newUpdateStatus;
			UpdateStatusChanged?.Invoke(this, newUpdateStatus);
		}
	}

	public override void Dispose()
	{
		Synchronizer.PropertyChanged -= Synchronizer_PropertyChanged;
		base.Dispose();
	}
}
