using System;
using System.Reactive;
using ReactiveUI;

namespace TimeServer.Services
{
	public class BinaryConfigurationSuspensionDriver : ISuspensionDriver
	{
		private readonly string _path;

		public BinaryConfigurationSuspensionDriver(string path) => _path = path;


		public IObservable<object> LoadState()
		{
			throw new NotImplementedException();
		}

		public IObservable<Unit> SaveState(object state)
		{
			throw new NotImplementedException();
		}

		public IObservable<Unit> InvalidateState()
		{
			throw new NotImplementedException();
		}
	}
}
