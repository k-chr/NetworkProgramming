namespace TimeClient.Services
{
	public interface IProtocol
	{
		HeaderType Header { get; set; }
		ActionType Action { get; set; }
		byte[] GetBytes();
	}
}
