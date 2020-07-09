using System;

namespace CustomControls.Models
{
	public class ClientModel
	{
		public string Id { get; set; }
		public string Ip { get; }
		public int Port { get; }
		public string Connected { get; set; }
		public string Shorten => Id.Substring(0, 1);

		public ClientModel(ValueTuple<string, string, int> info)
		{
			(Ip, Id, Port) = info;
			Id = string.IsNullOrEmpty(Id) ? $"Client_{Guid.NewGuid()}" : Id;
		}

		public override string ToString()
		{
			return $"[Id]: {Id}\n[Ip]: {Ip}\n[Port]: {Port}\n";
		}
	}
}