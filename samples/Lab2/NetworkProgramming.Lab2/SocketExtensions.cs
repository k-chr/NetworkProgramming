using System.Net.Sockets;
using System.Reflection;

namespace NetworkProgramming.Lab2
{
	public static class SocketExtensions
	{
		public static bool IsDisposed(this Socket socket)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty;
			var field = socket.GetType().GetProperty("CleanedUp", flags);
			return (bool) (field?.GetValue(socket, null) ?? false);
		}
	}
}