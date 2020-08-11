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
			if (!(selectedServer is null) && !old.Equals(selectedServer))
				ConfigViewModel.SelectedServer = selectedServer;
		}

		public void OnClosing()
		{
			_client?.StopService();
			AccessibleServers?.Clear();
		}

		public MainWindowViewModel(IManagedNotificationManager managedNotificationManager,
			ConfigViewModel configViewModel)
		{
			_managedNotificationManager = managedNotificationManager;
			ConfigViewModel = configViewModel;
		}

		public ObservableCollection<ServerModel> AccessibleServers { get; } = new ObservableCollection<ServerModel>();
	}
}