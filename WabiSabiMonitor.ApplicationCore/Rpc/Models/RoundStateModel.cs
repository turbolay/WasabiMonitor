using WabiSabiMonitor.ApplicationCore.Utils.Affiliation.Models;
using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models;

namespace WabiSabiMonitor.ApplicationCore.Rpc.Models;

public record RoundStateModel(RoundState[] RoundStates, CoinJoinFeeRateMedian[] CoinJoinFeeRateMedians, AffiliateInformation AffiliateInformation);