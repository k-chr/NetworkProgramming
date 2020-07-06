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
		public Client(Socket socket, IReporter lastException, IReporter lastMessage, IReporter disconnected) : base(
			socket, lastException, lastMessage, disconnected)
		{
		}

		public override void Send(string message, string to = "")
		{
			throw new NotImplementedException();
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
			var message = Encoding.UTF8.GetString(stream.ToArray());
			OnNewMessage((message, "", "").ToTuple());
		}

		public override void StopService()
		{
			throw new NotImplementedException();
		}

		public override void StartService()
		{
			Receive();
		}
	}
}