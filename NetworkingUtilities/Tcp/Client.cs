using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.StateObjects;

namespace NetworkingUtilities.Tcp
{
	public class Client : AbstractClient
	{
		public Client(Socket socket, bool serverHandler) : base(socket, serverHandler)
		{
		}

		public Client(string address, in int port) : base(new Socket(SocketType.Stream, ProtocolType.Tcp))
		{
			Connect(address, port);
		}

		private void Connect(string address, in int port)
		{
			try
			{
				ClientSocket.BeginConnect(address, port, OnConnectCallback, ClientSocket);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception);
			}
		}

		private void OnConnectCallback(IAsyncResult ar)
		{
			if (ar.AsyncState is Socket socket)
			{
				try
				{
					 socket.EndConnect(ar);
				}
				catch (ObjectDisposedException)
				{
				}
				catch (SocketException socketException)
				{
					OnCaughtException(socketException);
				}
				catch (Exception exception)
				{
					OnCaughtException(exception);
				}
			}
		}

		public override void Send(string message, string to = "") => Send(ClientSocket, message);

		private void Send(Socket clientSocket, string message)
		{
			var bytes = Encoding.ASCII.GetBytes(message);

			try
			{
				clientSocket.BeginSend(bytes, 0, bytes.Length, 0, OnSendCallback, clientSocket);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException);
			}
			catch (Exception e)
			{
				OnCaughtException(e);
			}
		}

		private void OnSendCallback(IAsyncResult ar)
		{
			try
			{
				if (ar.AsyncState is Socket socket)
				{
					var _ = socket.EndSend(ar);
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception);
			}
		}

		public override void Receive()
		{
			var state = new ControlState
			{
				CurrentSocket = ClientSocket,
				Buffer = new byte[MaxBufferSize],
				BufferSize = MaxBufferSize
			};
			Receive(ClientSocket, state);
		}

		private void Receive(Socket clientSocket, ControlState state)
		{
			try
			{
				clientSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnReceiveCallback, state);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception);
			}
		}

		private void OnReceiveCallback(IAsyncResult ar)
		{
			if (ar.AsyncState is ControlState state)
			{
				var clientSocket = state.CurrentSocket;

				try
				{
					var bytesRead = clientSocket.EndReceive(ar);
					if (bytesRead > 0)
					{
						state.StreamBuffer.Write(state.Buffer, 0, bytesRead);
						if (state.Buffer.Any(@byte => @byte == '\0'))
						{
							ProcessMessage(state.StreamBuffer);
							state.StreamBuffer = new MemoryStream();
						}
					}
					else if (state.StreamBuffer.CanWrite && state.StreamBuffer.Length > 0)
					{
						ProcessMessage(state.StreamBuffer);
						state.StreamBuffer = new MemoryStream();
					}
					else
					{
						throw new Exception("Issues with receiving message from sender");
					}

					Receive(clientSocket, state);
				}
				catch (ObjectDisposedException)
				{
				}
				catch (SocketException socketException)
				{
					OnCaughtException(socketException);
				}
				catch (Exception exception)
				{
					OnCaughtException(exception);
				}
			}
		}

		private void ProcessMessage(MemoryStream stateStreamBuffer)
		{
			using var stream = stateStreamBuffer;
			stream.Seek(0, SeekOrigin.Begin);
			var message = Encoding.UTF8.GetString(stream.ToArray()).Trim();
			var (from, to) = ServerHandler ? (WhoAmI.Id, "server") : ("server", WhoAmI.Id);
			OnNewMessage((message, from, to).ToTuple());
		}

		public override void StopService() => Disconnect(ClientSocket);

		public override void StartService() => Receive();

		private void Disconnect(Socket clientSocket)
		{
			try
			{
				clientSocket.Shutdown(SocketShutdown.Both);
				clientSocket.BeginDisconnect(true, OnDisconnectCallback, clientSocket);
			}
			catch (ObjectDisposedException)
			{
				OnDisconnect((WhoAmI.Ip, WhoAmI.Id, WhoAmI.Port).ToTuple());
			}
			catch (SocketException s)
			{
				OnCaughtException(s);
				OnDisconnect((WhoAmI.Ip, WhoAmI.Id, WhoAmI.Port).ToTuple());
			}
			catch (Exception e)
			{
				OnCaughtException(e);
				OnDisconnect((WhoAmI.Ip, WhoAmI.Id, WhoAmI.Port).ToTuple());
			}
		}

		private void OnDisconnectCallback(IAsyncResult ar)
		{
			if (ar.AsyncState is Socket socket)
			{
				try
				{
					socket.EndDisconnect(ar);
					socket.Close(2000);
				}
				catch (ObjectDisposedException)
				{
				}
				catch (SocketException socketException)
				{
					OnCaughtException(socketException);
				}
				finally
				{
					OnDisconnect((WhoAmI.Ip, WhoAmI.Id, WhoAmI.Port).ToTuple());
				}
			}
		}
	}
}