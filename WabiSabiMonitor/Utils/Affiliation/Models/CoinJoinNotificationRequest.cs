using WabiSabiMonitor.Utils.Affiliation.Models.CoinjoinNotification;

namespace WabiSabiMonitor.Utils.Affiliation.Models;

public record CoinJoinNotificationRequest(Body Body, byte[] Signature);
