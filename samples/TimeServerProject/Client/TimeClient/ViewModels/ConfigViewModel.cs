using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NetworkingUtilities.Extensions;
using ReactiveUI;

namespace TimeClient.ViewModels
{
	public class ConfigViewModel : ViewModelBase, INotifyDataErrorInfo
	{
		private string _multicastAddress;
		private int _multicastPort;
		private int _localPort;
		private int _discoveryQueryPeriod;
		private int _timeQueryPeriod;


		private Dictionary<string, string> _propertiesErrors = new Dictionary<string, string>
		{
		};


		public string MulticastAddress
		{
			get => _multicastAddress;
			set => this.RaiseAndSetIfChanged(ref _multicastAddress, value);
		}

		public int MulticastPort
		{
			get => _multicastPort;
			set => this.RaiseAndSetIfChanged(ref _multicastPort, value);
		}

		public int LocalPort
		{
			get => _localPort;
			set => this.RaiseAndSetIfChanged(ref _localPort, value);
		}

		public int TimeQueryPeriod
		{
			get => _timeQueryPeriod;
			set => this.RaiseAndSetIfChanged(ref _timeQueryPeriod, value);
		}

		public int DiscoveryQueryPeriod
		{
			get => _discoveryQueryPeriod;
			set => this.RaiseAndSetIfChanged(ref _discoveryQueryPeriod, value);
		}

		public IEnumerable GetErrors(string propertyName) => new[] {_propertiesErrors.Get(propertyName)};

		public bool HasErrors { get; }

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
	}
}