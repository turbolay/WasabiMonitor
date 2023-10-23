using WabiSabiMonitor.Utils.Affiliation.Models;
using WabiSabiMonitor.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.Rpc.Models;

public record RoundStateResponse(RoundState[] RoundStates, CoinJoinFeeRateMedian[] CoinJoinFeeRateMedians, AffiliateInformation AffiliateInformation);