using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using ReactiveUI;
using TimeClient.Models;

namespace TimeClient.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private readonly IManagedNotificationManager _managedNotificationManager;

		private Services.TimeClient _client;

		private ServerModel _selectedServer;
		private bool _isValid;

		public ConfigViewModel ConfigViewModel { get; }

		public ServerModel SelectedServer
		{
			get => _selectedServer;
			set
			{
				var old = _selectedServer;
				this.RaiseAndSetIfChanged(ref _selectedServer, value);
				OnSelectedServerChanged(old, _selectedServer);
			}
		}

		public bool IsValid
		{
			get => _isValid;
			set => this.RaiseAndSetIfChanged(ref _isValid, value);
		}

		private void OnSelectedServerChanged(ServerModel old, ServerModel selectedServer)
		{
			if(!old.Equals(selectedServer))
				ConfigViewModel.SelectedServer = selectedServer;
		}

		public void OnClosing()
		{
			_client?.StopService();
			AccessibleServers?.Clear();
			ConfigViewModel?.Save();
		}

		public MainWindowViewModel(IManagedNotificationManager managedNotificationManager)
		{
			_managedNotificationManager = managedNotificationManager;
			ConfigViewModel = new ConfigViewModel();
		}

		public ObservableCollection<ServerModel> AccessibleServers { get; } = new ObservableCollection<ServerModel>();

	}
}