using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ReactiveUI;

namespace TimeClient.ViewModels
{
	public class ConfigViewModel : ViewModelBase, INotifyDataErrorInfo
	{
		private string _multicastAddress;
		private int _multicastPort;


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

		public IEnumerable GetErrors(string propertyName)
		{
			throw new NotImplementedException();
		}

		public bool HasErrors { get; }
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
	}
}
