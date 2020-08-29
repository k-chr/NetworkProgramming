using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NetworkingUtilities.Extensions;
using ReactiveUI;
using TimeProjectServices.Exceptions;
using TimeProjectServices.ViewModels;

namespace TimeServer.ViewModels
{
	public class ConfigViewModel : ViewModelBase, INotifyDataErrorInfo
	{
		private string _multicastAddress;
		private int _multicastPort;
		private bool _backedUp;
		private (string MulticastAddress, int MulticastPort) _backup;

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

		public bool BackedUp
		{
			get => _backedUp;
			set => this.RaiseAndSetIfChanged(ref _backedUp, value);
		}

		private static readonly Dictionary<string, string> DefaultPropertiesErrors = new Dictionary<string, string>
		{
			{nameof(MulticastPort), "Invalid multicast port value"},
			{nameof(MulticastAddress), "Provided text is not multicast address"},
		};

		private readonly Dictionary<string, string> _propertiesErrors = new Dictionary<string, string>();

		public ConfigViewModel(string multicastAddress = "", int multicastPort = 0) : this()
		{
			_multicastPort = multicastPort;
			_multicastAddress = multicastAddress;

			this.WhenAnyValue(model => model.MulticastPort)
			   .Subscribe(val => UpdateErrorInformation(nameof(MulticastPort), val));
			this.WhenAnyValue(model => model.MulticastAddress)
			   .Subscribe(s => UpdateErrorInformation(nameof(MulticastAddress), s));
		}

		private ConfigViewModel()
		{
			PropertyChanging += (sender, args) =>
			{
				if (!BackedUp && !args.PropertyName.Equals(nameof(BackedUp)))
				{
					_backup.MulticastAddress = MulticastAddress;
					_backup.MulticastPort = MulticastPort;
					BackedUp = !BackedUp;
				}
			};
		}

		private void UpdateErrorInformation(string propertyName, object val)
		{
			Func<bool> test = propertyName switch
							  {
								  nameof(MulticastPort) => () => ((int) val).InRange(ushort.MinValue, ushort.MaxValue),
								  nameof(MulticastAddress) => () => ((string) val).IsMulticastAddress(),
								  _ => throw new PropertyNotFoundException($"Property: {propertyName} not registered")
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

		public IEnumerable GetErrors(string propertyName) => new[] {_propertiesErrors.Get(propertyName)};
		public bool HasErrors => _propertiesErrors.Any();
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
		public event EventHandler ConfigurationChanged;

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
			(MulticastAddress, MulticastPort) = _backup;
			_backup = default;
			BackedUp = default;
		}

		private void OnConfigurationChanged() => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
	}
}