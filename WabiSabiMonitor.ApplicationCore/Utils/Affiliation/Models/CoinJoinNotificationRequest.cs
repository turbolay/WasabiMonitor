using WabiSabiMonitor.ApplicationCore.Utils.Affiliation.Models.CoinjoinNotification;

namespace WabiSabiMonitor.ApplicationCore.Utils.Affiliation.Models;

public record CoinJoinNotificationRequest(Body Body, byte[] Signature);
