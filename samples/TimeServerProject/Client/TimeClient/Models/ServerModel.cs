using System;
using System.Net;

namespace TimeClient.Models
{
	public class ServerModel
	{
		public static implicit operator ServerModel(ValueTuple<IPEndPoint, string> pair) =>
			Create(pair.Item1, pair.Item2);

		public static ServerModel Create(IPEndPoint pairItem1, string pairItem2) =>
			new ServerModel
			{
				Ip = pairItem1,
				Name = pairItem2
			};

		public IPEndPoint Ip { get; private set; }

		public string Name { get; private set; }

		public override string ToString() => $"{Name}|{Ip}";
	}
}