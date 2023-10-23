using WabiSabiMonitor.Utils.WabiSabi.Models.MultipartyTransaction;

namespace WabiSabiMonitor.Utils.WabiSabi.Models.Serialization;

public class CoinJoinEventJsonConverter : GenericInterfaceJsonConverter<IEvent>
{
	public CoinJoinEventJsonConverter() : base(new[] { typeof(InputAdded), typeof(OutputAdded), typeof(RoundCreated) })
	{
	}
}
