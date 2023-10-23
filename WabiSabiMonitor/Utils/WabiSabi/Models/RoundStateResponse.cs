using WabiSabiMonitor.Utils.Affiliation.Models;

namespace WabiSabiMonitor.Utils.WabiSabi.Models;

public record RoundStateResponse(RoundState[] RoundStates, CoinJoinFeeRateMedian[] CoinJoinFeeRateMedians, AffiliateInformation AffiliateInformation);
