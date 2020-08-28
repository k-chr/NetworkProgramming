using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
			new Socket(SocketType.Stream, ProtocolType.Tcp)) =>
			Connect(address, port, manualResetEvent);

		public Client() : base(new Socket(SocketType.Stream, ProtocolType.Tcp))
		{
		}

		public void Connect(string address, in int port, ManualResetEvent manualResetEvent)
		{
			try
			{
				OnReportingStatus(StatusCode.Info, $"Attempt to create TCP connection to {address}:{port} host");
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
						WhoAmI = new ClientEvent("", endPoint, ClientSocket.RemoteEndPoint as IPEndPoint);
					}

					OnReportingStatus(StatusCode.Success,
						$"Successfully created TCP connection to {ClientSocket.RemoteEndPoint}");
					OnConnect(WhoAmI.Id, WhoAmI.Ip, WhoAmI.ServerIp);
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

		public override void Send(byte[] message, string to = "") => Send(ClientSocket, message);

		private void Send(Socket clientSocket, byte[] bytes)
		{
			try
			{
				OnReportingStatus(StatusCode.Info, $"Started sending {bytes.Length} bytes to remote TCP end-point");
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
					OnReportingStatus(StatusCode.Success, $"Successfully send {_} bytes to remote TCP end-point");
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
				OnReportingStatus(StatusCode.Info, "Started receiving bytes from remote TCP end-point");
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
					OnReportingStatus(StatusCode.Success,
						$"Successfully received {bytesRead} bytes from remote TCP end-point");
					if (bytesRead > 0)
					{
						state.StreamBuffer.Write(state.Buffer, 0, bytesRead);
						state.Buffer = new byte[MaxBufferSize];
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
						Disconnect(clientSocket);
						return;
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

		public override void StopService() => Disconnect(ClientSocket);

		public override void StartService() => Receive();

		private void Disconnect(Socket clientSocket)
		{
			try
			{
				OnReportingStatus(StatusCode.Info, "Attempt to disconnect from remote TCP end-point");
				clientSocket.Shutdown(SocketShutdown.Both);
				if (ServerHandler)
				{
					OnDisconnect(WhoAmI.Id, WhoAmI.Ip, WhoAmI.ServerIp);
					clientSocket.Close();
				}
				else
				{
					clientSocket.BeginDisconnect(false, OnDisconnectCallback, clientSocket);
				}
			}
			catch (ObjectDisposedException)
			{
				OnDisconnect(WhoAmI.Id, WhoAmI.Ip, WhoAmI.ServerIp);
			}
			catch (SocketException s)
			{
				OnCaughtException(s, EventCode.Disconnect);
				OnDisconnect(WhoAmI.Id, WhoAmI.Ip, WhoAmI.ServerIp);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
				OnDisconnect(WhoAmI.Id, WhoAmI.Ip, WhoAmI.ServerIp);
			}
		}

		private void OnDisconnectCallback(IAsyncResult ar)
		{
			if (ar.AsyncState is Socket socket)
			{
				try
				{
					socket.EndDisconnect(ar);
					OnReportingStatus(StatusCode.Success, "Disconnected successfully from remote end-point");
					socket.Close(1000);
					OnReportingStatus(StatusCode.Success, "Successfully disposed TCP socket");
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
					OnDisconnect(WhoAmI.Id, WhoAmI.Ip, WhoAmI.ServerIp);
				}
			}
		}
	}
}