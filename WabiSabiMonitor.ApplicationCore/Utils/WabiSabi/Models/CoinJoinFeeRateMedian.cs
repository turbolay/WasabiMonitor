using NBitcoin;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

public record CoinJoinFeeRateMedian(TimeSpan TimeFrame, FeeRate MedianFeeRate);
