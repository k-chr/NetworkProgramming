using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using NetworkProgramming.Lab2;
using NetworkProgramming.Lab3.Models;

namespace NetworkProgramming.Lab3.Services
{
	public class ClientHandler
	{
		protected string OnSendErrorMessage => "Cannot send provided message to client due to connection issues!\n";
		protected string OnDisconnectSuccessMessage => $"Succeeded in disconnecting current connection with: {_data}\n";
		protected string OnDisconnectErrorMessage => "Failed to disconnect\n";
		protected string OnReceiveErrorMessage => "Failed to receive message due to connection issues\n";

		private readonly Socket _socket;
		private readonly ClientModel _data;
		private const int MaxLen = 1024;

		public ClientHandler(Socket connectedSocket, ClientModel data)
		{
			_data = data;
			_socket = connectedSocket;
			Receive(_socket);
		}

		public event EventHandler<object[]> OnLogEvent;
		public event EventHandler ClientDisconnected;

		public bool IsConnected() =>
			!(_socket.IsDisposed() || (_socket.Poll(1000, SelectMode.SelectRead) && (_socket.Available == 0)) ||
			  !_socket.Connected);


		private void Receive(Socket socket)
		{
			var state = new ControlState
			{
				CurrentSocket = socket,
				Buffer = new byte[MaxLen],
				BufferSize = MaxLen
			};
			try
			{
				socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0,
					ReceiveCallback, state);
			}
			catch (Exception exception)
			{
				if (!_socket.IsDisposed())
				{
					OnLogEvent?.Invoke(this,
						new object[] {(int) Logger.MessageType.Error, exception, OnReceiveErrorMessage});
					Disconnect();
				}
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			var state = (ControlState) ar.AsyncState;
			var client = state.CurrentSocket;

			try
			{
				var bytesRead = client.EndReceive(ar);

				if (bytesRead > 0)
				{
					state.StreamBuffer.Write(state.Buffer, 0, bytesRead);
					if (state.Buffer.Any(byte_ => byte_ == '\0'))
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
					Disconnect();
				}
			}
			catch (Exception s)
			{
				if (!_socket.IsDisposed())
				{
					Disconnect();
					OnLogEvent?.Invoke(this, new object[] {(int) Logger.MessageType.Error, s, OnReceiveErrorMessage});
				}
			}

			state.Buffer = new byte[state.BufferSize];


			try
			{
				client.BeginReceive(state.Buffer, 0, (int) state.BufferSize, 0, ReceiveCallback, state);
			}
			catch (Exception exception)
			{
				if (!_socket.IsDisposed())
				{
					OnLogEvent?.Invoke(this,
						new object[] {(int) Logger.MessageType.Error, exception, OnReceiveErrorMessage});
				}

				Disconnect();
			}
		}

		private void ProcessMessage(MemoryStream memory)
		{
			using var stream = memory;
			stream.Seek(0, SeekOrigin.Begin);
			var message = Encoding.UTF8.GetString(stream.ToArray());
			OnLogEvent?.Invoke(this, new object[] {(int) Logger.MessageType.Server, message.Trim()});
		}

		public void Send(string msg)
		{
			Send(_socket, msg);
			OnLogEvent?.Invoke(this, new object[] {(int) Logger.MessageType.Client, msg.Trim()});
		}

		private void Send(Socket socket, string data)
		{
			var byteData = Encoding.ASCII.GetBytes(data);

			try
			{
				socket.BeginSend(byteData, 0, byteData.Length, 0,
					SendCallback, socket);
			}
			catch (Exception exception)
			{
				if (!_socket.IsDisposed())
				{
					Disconnect();
					OnLogEvent?.Invoke(this,
						new object[] {(int) Logger.MessageType.Error, exception, OnSendErrorMessage});
				}
			}
		}

		private void SendCallback(IAsyncResult ar)
		{
			try
			{
				var socket = (Socket) ar.AsyncState;
				var _ = socket.EndSend(ar);
			}
			catch (Exception e)
			{
				if (!_socket.IsDisposed())
				{
					Disconnect();
					OnLogEvent?.Invoke(this, new object[] {(int) Logger.MessageType.Error, e, OnSendErrorMessage});
				}
			}
		}

		public void Disconnect()
		{
			Disconnect(_socket);
		}

		private void Disconnect(Socket socket)
		{
			try
			{
				socket.Shutdown(SocketShutdown.Both);
				socket.BeginDisconnect(true, DisconnectCallback, socket);
			}
			catch (Exception e)
			{
				if (!socket.IsDisposed())
					OnLogEvent?.Invoke(this,
						new object[] {(int) Logger.MessageType.Error, e, OnDisconnectErrorMessage});
			}
		}

		private void DisconnectCallback(IAsyncResult ar)
		{
			var socket = (Socket) ar.AsyncState;

			try
			{
				socket.EndDisconnect(ar);
			}
			catch (Exception e)
			{
				// ignored
			}
			finally
			{
				socket.Close(2000);
				OnLogEvent?.Invoke(this, new object[] {(int) Logger.MessageType.Success, OnDisconnectSuccessMessage});
				ClientDisconnected?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}