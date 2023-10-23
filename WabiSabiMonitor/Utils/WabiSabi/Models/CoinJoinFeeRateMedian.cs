using NBitcoin;

namespace WabiSabiMonitor.Utils.WabiSabi.Models;

public record CoinJoinFeeRateMedian(TimeSpan TimeFrame, FeeRate MedianFeeRate);
