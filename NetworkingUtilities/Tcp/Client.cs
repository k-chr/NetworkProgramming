using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;
using NetworkingUtilities.Utilities.StateObjects;

namespace NetworkingUtilities.Tcp
{
	public class Client : AbstractClient
	{
		public Client(Socket socket, bool serverHandler) : base(socket, serverHandler)
		{
		}

		public Client(string address, in int port, ManualResetEvent manualResetEvent) : base(
			new Socket(SocketType.Stream, ProtocolType.Tcp))
		{
			Connect(address, port, manualResetEvent);
		}

		private void Connect(string address, in int port, ManualResetEvent manualResetEvent)
		{
			try
			{
				ClientSocket.BeginConnect(address, port, OnConnectCallback,
					new WaitState {ClientSocket = ClientSocket, BlockingEvent = manualResetEvent});
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Connect);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}

		private void OnConnectCallback(IAsyncResult ar)
		{
			if (ar.AsyncState is WaitState state)
			{
				try
				{
					var socket = state.ClientSocket;
					socket.EndConnect(ar);
					state.BlockingEvent.Set();
					if (ClientSocket.LocalEndPoint is IPEndPoint endPoint)
					{
						WhoAmI = new ClientEvent(endPoint.Address, endPoint.Port);
					}

					OnConnect(WhoAmI.Ip, WhoAmI.Id, WhoAmI.Port);
				}
				catch (ObjectDisposedException)
				{
				}
				catch (SocketException socketException)
				{
					OnCaughtException(socketException, EventCode.Connect);
				}
				catch (Exception exception)
				{
					OnCaughtException(exception, EventCode.Other);
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
				OnCaughtException(socketException, EventCode.Send);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
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
				OnCaughtException(socketException, EventCode.Send);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
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
				OnCaughtException(socketException, EventCode.Receive);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
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
						Disconnect(ClientSocket);
					}

					Receive(clientSocket, state);
				}
				catch (ObjectDisposedException)
				{
				}
				catch (SocketException socketException)
				{
					OnCaughtException(socketException, EventCode.Receive);
					Disconnect(ClientSocket);
				}
				catch (Exception exception)
				{
					OnCaughtException(exception, EventCode.Other);
				}
			}
		}

		private void ProcessMessage(MemoryStream stateStreamBuffer)
		{
			using var stream = stateStreamBuffer;
			stream.Seek(0, SeekOrigin.Begin);
			var message = Encoding.UTF8.GetString(stream.ToArray()).Trim();
			var (from, to) = ServerHandler ? (WhoAmI.Id, "server") : ("server", WhoAmI.Id);
			OnNewMessage(message, from, to);
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
				OnDisconnect(WhoAmI.Ip, WhoAmI.Id, WhoAmI.Port);
			}
			catch (SocketException s)
			{
				OnCaughtException(s, EventCode.Disconnect);
				OnDisconnect(WhoAmI.Ip, WhoAmI.Id, WhoAmI.Port);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
				OnDisconnect(WhoAmI.Ip, WhoAmI.Id, WhoAmI.Port);
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
					OnCaughtException(socketException, EventCode.Disconnect);
				}
				catch (Exception e)
				{
					OnCaughtException(e, EventCode.Other);
				}
				finally
				{
					OnDisconnect(WhoAmI.Ip, WhoAmI.Id, WhoAmI.Port);
				}
			}
		}
	}
}