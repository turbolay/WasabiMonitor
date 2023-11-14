using NBitcoin;

namespace WabiSabiMonitor.ApplicationCore.Utils.Affiliation;

public record AffiliateInput(OutPoint Prevout, Script ScriptPubKey, Money Amount, string AffiliationId, bool IsNoFee);
