using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NetworkingUtilities.Extensions;
using ReactiveUI;
using TimeClient.Exceptions;
using TimeClient.Models;

namespace TimeClient.ViewModels
{
	public class ConfigViewModel : ViewModelBase, INotifyDataErrorInfo
	{
		private string _multicastAddress;
		private int _multicastPort;
		private int _localPort;
		private int _discoveryQueryPeriod;
		private int _timeQueryPeriod;
		private bool _backedUp;
		
		private (string MulticastAddress, int MulticastPort, int LocalPort, int DiscoveryQueryPeriod, int
			TimeQueryPeriod) _backup;

		private static readonly Dictionary<string, string> DefaultPropertiesErrors = new Dictionary<string, string>
		{
			{nameof(LocalPort), "Invalid port number"},
			{nameof(MulticastPort), "Invalid multicast port value"},
			{nameof(MulticastAddress), "Provided text is not multicast address"},
			{nameof(DiscoveryQueryPeriod), "Discovery query period should be greater than 1"},
			{nameof(TimeQueryPeriod), "Time query period should be in range <10, 1000>"}
		};

		private readonly Dictionary<string, string> _propertiesErrors = new Dictionary<string, string>();


		public ConfigViewModel(string multicastAddress = "", int multicastPort = 0, int localPort = 0,
			int timeQueryPeriod = 0, int discoveryQueryPeriod = 0) : this()
		{
			_multicastPort = multicastPort;
			_multicastAddress = multicastAddress;
			_localPort = localPort;
			_timeQueryPeriod = timeQueryPeriod;
			_discoveryQueryPeriod = discoveryQueryPeriod;

			this.WhenAnyValue(model => model.MulticastPort)
			   .Subscribe(val => UpdateErrorInformation(nameof(MulticastPort), val));
			this.WhenAnyValue(model => model.DiscoveryQueryPeriod)
			   .Subscribe(i => UpdateErrorInformation(nameof(DiscoveryQueryPeriod), i));
			this.WhenAnyValue(model => model.MulticastAddress)
			   .Subscribe(s => UpdateErrorInformation(nameof(MulticastAddress), s));
			this.WhenAnyValue(model => model.TimeQueryPeriod)
			   .Subscribe(i => UpdateErrorInformation(nameof(TimeQueryPeriod), i));
			this.WhenAnyValue(model => model.LocalPort)
			   .Subscribe(i => UpdateErrorInformation(nameof(LocalPort), i));
		}

		private ConfigViewModel()
		{
			PropertyChanging += (sender, args) =>
			{
				if (!BackedUp && !args.PropertyName.Equals(nameof(BackedUp)))
				{
					_backup.DiscoveryQueryPeriod = DiscoveryQueryPeriod;
					_backup.LocalPort = LocalPort;
					_backup.TimeQueryPeriod = TimeQueryPeriod;
					_backup.MulticastAddress = MulticastAddress;
					_backup.MulticastPort = MulticastPort;
					BackedUp = !BackedUp;
				}
			};
		}

		private void UpdateErrorInformation(string propertyName, object value)
		{
			Func<bool> test = propertyName switch
							  {
								  var name when
								  name.Equals(nameof(MulticastPort)) || name.Equals(nameof(LocalPort)) =>
								  () => ((int) value).InRange(0, ushort.MaxValue),
								  nameof(MulticastAddress) => () => ((string) value).IsMulticastAddress(),
								  nameof(DiscoveryQueryPeriod) => () => (int) value > 0,
								  nameof(TimeQueryPeriod) => () => ((int) value).InRange(10, 1000),
								  _ => throw new PropertyNotFoundException(
									  $"Provided property: {propertyName} is not registered")
							  };

			if (test())
			{
				if (_propertiesErrors.ContainsKey(propertyName))
				{
					_propertiesErrors.Remove(propertyName);
					this.RaisePropertyChanged(nameof(HasErrors));
					ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
				}
			}
			else
			{
				if (!_propertiesErrors.ContainsKey(propertyName))
				{
					_propertiesErrors.Add(propertyName, DefaultPropertiesErrors[propertyName]);
					this.RaisePropertyChanged(nameof(HasErrors));
					ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
				}
			}
		}

		public bool BackedUp
		{
			get => _backedUp;
			set => this.RaiseAndSetIfChanged(ref _backedUp, value);
		}

		public ServerModel SelectedServer { get; set; }

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

		public bool HasErrors => _propertiesErrors.Any();

		public IEnumerable GetErrors(string propertyName) => new[] {_propertiesErrors.Get(propertyName)}; 
		
		public event EventHandler ConfigurationChanged;
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public void ApplyConfiguration()
		{
			if (!HasErrors)
			{
				OnConfigurationChanged();
				_backup = default;
				BackedUp = default;
			}
		}

		public void DiscardConfiguration()
		{
			(MulticastAddress, MulticastPort, LocalPort, DiscoveryQueryPeriod, TimeQueryPeriod) = _backup;
			_backup = default;
			BackedUp = default;
		}

		private void OnConfigurationChanged() => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
	}
}