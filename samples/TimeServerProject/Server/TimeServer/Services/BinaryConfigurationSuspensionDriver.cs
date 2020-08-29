using System;
using System.IO;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using TimeServer.ViewModels;

namespace TimeServer.Services
{
	public class BinaryConfigurationSuspensionDriver : ISuspensionDriver
	{
		private readonly string _path;

		public BinaryConfigurationSuspensionDriver(string path) => _path = path;


		public IObservable<object> LoadState()
		{
			ConfigViewModel configViewModel;
			try
			{
				var bytes = File.ReadAllBytes(_path);

				var multicastPort = bytes[..4];
				var multicastAddress = bytes[4..8];

				configViewModel = new ConfigViewModel
				(
					multicastPort: BitConverter.ToInt32(multicastPort),
					multicastAddress: new IPAddress(multicastAddress).ToString()
				);
			}
			catch (Exception)
			{
				configViewModel = new ConfigViewModel
				(
					multicastPort: 7,
					multicastAddress: "224.0.0.0"
				);
			}

			return Observable.Return(configViewModel);
		}

		public IObservable<Unit> SaveState(object state)
		{
			if (state is ConfigViewModel model && model.HasErrors == false)
			{
				var stream = new MemoryStream();
				stream.Write(BitConverter.GetBytes(model.MulticastPort), 0, 4);
				stream.Write(IPAddress.Parse(model.MulticastAddress).GetAddressBytes(), 0, 4);
				stream.Seek(0, SeekOrigin.Begin);
				File.WriteAllBytes(_path, stream.ToArray());
			}

			return Observable.Return(Unit.Default);
		}

		public IObservable<Unit> InvalidateState()
		{
			if (File.Exists(_path))
				File.Delete(_path);
			return Observable.Return(Unit.Default);
		}
	}
}