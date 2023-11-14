using System.Collections.Immutable;
using System.ComponentModel;

namespace WabiSabiMonitor.ApplicationCore.Utils.Affiliation.Serialization;

public class DefaultAffiliateServersAttribute : DefaultValueAttribute
{
	public DefaultAffiliateServersAttribute() : base(ImmutableDictionary<string, string>.Empty)
	{
	}
}
