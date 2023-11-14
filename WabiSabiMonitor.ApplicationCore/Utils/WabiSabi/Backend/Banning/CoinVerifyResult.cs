using NBitcoin;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Backend.Banning;

public record CoinVerifyResult(Coin Coin, bool ShouldBan, bool ShouldRemove);
