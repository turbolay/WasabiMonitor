using System.Text;
using WabiSabiMonitor.ApplicationCore.Utils.Helpers;
using WabiSabiMonitor.ApplicationCore.Utils.Tor.Socks5.Models.Bases;

namespace WabiSabiMonitor.ApplicationCore.Utils.Tor.Socks5.Models.Fields.ByteArrayFields;

public class UNameField : ByteArraySerializableBase
{
	public UNameField(byte[] bytes)
	{
		Bytes = Guard.NotNullOrEmpty(nameof(bytes), bytes);
	}

	public UNameField(string uName)
		: this(Encoding.UTF8.GetBytes(uName))
	{
	}

	private byte[] Bytes { get; }

	public override byte[] ToBytes() => Bytes;
}
