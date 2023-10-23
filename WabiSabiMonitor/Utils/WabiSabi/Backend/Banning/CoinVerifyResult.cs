using NBitcoin;

namespace WabiSabiMonitor.Utils.WabiSabi.Backend.Banning;

public record CoinVerifyResult(Coin Coin, bool ShouldBan, bool ShouldRemove);
