using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using NetworkingUtilities.Abstracts;

namespace TimeClient.Services
{
	public class TimeClient : AbstractClient
	{
		public TimeClient(Socket clientSocket, bool serverHandler = false) : base(clientSocket, serverHandler)
		{
		}

		public override void Send(string message, string to = "")
		{
			throw new NotImplementedException();
		}

		public override void Receive()
		{
			throw new NotImplementedException();
		}

		public override void StopService()
		{
			throw new NotImplementedException();
		}

		public override void StartService()
		{
			throw new NotImplementedException();
		}
	}
}
