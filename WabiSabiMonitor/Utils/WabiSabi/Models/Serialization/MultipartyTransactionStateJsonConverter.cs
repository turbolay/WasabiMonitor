using WabiSabiMonitor.Utils.WabiSabi.Models.MultipartyTransaction;

namespace WabiSabiMonitor.Utils.WabiSabi.Models.Serialization;

public class MultipartyTransactionStateJsonConverter : GenericInterfaceJsonConverter<MultipartyTransactionState>
{
	public MultipartyTransactionStateJsonConverter() : base(new[] { typeof(ConstructionState), typeof(SigningState) })
	{
	}
}
