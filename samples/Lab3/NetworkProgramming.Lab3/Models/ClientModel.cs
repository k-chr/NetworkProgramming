using System;

namespace NetworkProgramming.Lab3.Models
{
	public class ClientModel
	{
		public string Id { get; }
		public string Ip { get; }
		public int Port { get; }

		public ClientModel(Tuple<int, string> info)
		{
			Id = $"Client_{Guid.NewGuid()}";
			(Port, Ip) = info;
		}

		public override string ToString()
		{
			return $"Id: {Id} | Ip: {Ip} | Port: {Port}";
		}
	}
}