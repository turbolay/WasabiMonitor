using System.Collections.Immutable;
using NBitcoin;

namespace WabiSabiMonitor.Utils.Blockchain.Keys;

public class HdPubKeyGlobalView : HdPubKeyPathView
{
	public HdPubKeyGlobalView(ImmutableList<HdPubKey> hdPubKeys)
		: base(hdPubKeys)
	{
	}

	internal HdPubKeyPathView GetChildKeyOf(KeyPath keyPath) =>
		new(Keys.Where(x => x.FullKeyPath.Parent == keyPath).ToImmutableList());
}
