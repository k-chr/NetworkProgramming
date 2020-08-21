using TimeProjectServices.Enums;

namespace TimeProjectServices.Protocols
{
	public interface IProtocol
	{
		HeaderType Header { get;  }
		ActionType Action { get; set; }
		byte[] GetBytes();
	}
}