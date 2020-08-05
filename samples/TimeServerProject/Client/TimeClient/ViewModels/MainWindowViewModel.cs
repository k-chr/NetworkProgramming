using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reactive.Linq;
using Avalonia.Controls.Notifications;
using NetworkingUtilities.Extensions;
using ReactiveUI;

namespace TimeClient.ViewModels
{
	public class MainWindowViewModel : ViewModelBase, INotifyDataErrorInfo
	{
		private readonly IManagedNotificationManager _managedNotificationManager;
		private Services.TimeClient _client;
		private string _selectedServer;
		private bool _isValid;

		public ConfigViewModel ConfigViewModel { get; }

		public string SelectedServer
		{
			get => _selectedServer;
			set
			{
				var old = _selectedServer;
				_selectedServer = value;
				OnSelectedServerChanged(old, _selectedServer);
			}
		}

		public bool IsValid
		{
			get => _isValid;
			set => this.RaiseAndSetIfChanged(ref _isValid, value);
		}

		private void OnSelectedServerChanged(string old, string selectedServer)
		{
		}

		public void OnClosing()
		{
		}

		public MainWindowViewModel(IManagedNotificationManager managedNotificationManager)
		{
			_managedNotificationManager = managedNotificationManager;
			ConfigViewModel = new ConfigViewModel();
		}

		public List<string> AccessibleServers { get; } = new List<string>
		{
			"TextBlock",
			"CheckBox",
			"ComboBox",
			"TextBox",
			"Calendar"
		};

		public IEnumerable GetErrors(string propertyName)
		{
			
		}

		public bool HasErrors { get; }
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
	}
}