using System.Collections.Immutable;
using NBitcoin;
using WabiSabiMonitor.Utils.Blockchain.TransactionOutputs;

namespace WabiSabiMonitor.Utils.WabiSabi.Client;

public abstract record CoinJoinResult;

public record SuccessfulCoinJoinResult(
	ImmutableList<SmartCoin> Coins,
	ImmutableList<Script> OutputScripts,
	Transaction UnsignedCoinJoin) : CoinJoinResult;

public record FailedCoinJoinResult : CoinJoinResult;

public record DisruptedCoinJoinResult(ImmutableList<SmartCoin> SignedCoins) : CoinJoinResult;
