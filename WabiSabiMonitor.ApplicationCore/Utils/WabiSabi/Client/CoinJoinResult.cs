using System.Collections.Immutable;
using NBitcoin;
using WabiSabiMonitor.ApplicationCore.Utils.Blockchain.TransactionOutputs;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Client;

public abstract record CoinJoinResult;

public record SuccessfulCoinJoinResult(
	ImmutableList<SmartCoin> Coins,
	ImmutableList<Script> OutputScripts,
	Transaction UnsignedCoinJoin) : CoinJoinResult;

public record FailedCoinJoinResult : CoinJoinResult;

public record DisruptedCoinJoinResult(ImmutableList<SmartCoin> SignedCoins) : CoinJoinResult;
