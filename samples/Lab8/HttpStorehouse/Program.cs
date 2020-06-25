using System;
using NetworkingUtilities.Http.Services;

namespace HttpStorehouse
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			var service = WebServer.Builder().WithPort(8080).WithPrefix(@"http://localhost")
			   .Build();

			Console.CancelKeyPress += (sender, o) =>
			{
				service.StopService();
				o.Cancel = true;
			};

			Console.WriteLine("Started listening press ctrl+c to close program");
			service.StartService();
			Console.WriteLine("Or press any key to stop listener");
			Console.ReadKey();
			service.StopService();
			Console.WriteLine("Press any key to close app");
			Console.ReadKey();
		}
	}
}