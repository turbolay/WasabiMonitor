using WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.MultipartyTransaction;

namespace WabiSabiMonitor.ApplicationCore.Utils.WabiSabi.Models.Serialization;

public class MultipartyTransactionStateJsonConverter : GenericInterfaceJsonConverter<MultipartyTransactionState>
{
	public MultipartyTransactionStateJsonConverter() : base(new[] { typeof(ConstructionState), typeof(SigningState) })
	{
	}
}
