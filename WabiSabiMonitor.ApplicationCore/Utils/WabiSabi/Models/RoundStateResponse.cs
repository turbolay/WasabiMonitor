using WabiSabiMonitor.ApplicationCore.Utils.Affiliation.Models;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

public record RoundStateResponse(RoundState[] RoundStates, CoinJoinFeeRateMedian[] CoinJoinFeeRateMedians, AffiliateInformation AffiliateInformation);
