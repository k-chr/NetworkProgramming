namespace TimeClient.Services
{
	public interface IProtocol
	{
		HeaderType Header { get;  }
		ActionType Action { get; set; }
		byte[] GetBytes();
	}
}
