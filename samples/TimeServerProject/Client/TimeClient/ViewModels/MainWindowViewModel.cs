using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CustomControls.Models;
using JetBrains.Annotations;
using NetworkingUtilities.Utilities.Events;
using ReactiveUI;
using TimeClient.Models;

namespace TimeClient.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private TimeProjectServices.Services.TimeClient _client;

		private ServerModel _selectedServer;

		private bool _isValid;

		private IManagedNotificationManager _managedNotificationManager;

		public ConfigViewModel ConfigViewModel { get; }

		public IManagedNotificationManager ManagedNotificationManager
		{
			get => _managedNotificationManager;
			set => this.RaiseAndSetIfChanged(ref _managedNotificationManager, value);
		}

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
			Logs?.Clear();
		}

		public MainWindowViewModel(IManagedNotificationManager managedNotificationManager,
			ConfigViewModel configViewModel)
		{
			_managedNotificationManager = managedNotificationManager;
			ConfigViewModel = configViewModel;

			ConfigViewModel.ErrorsChanged += (sender, args) =>
			{
				if (!ConfigViewModel.HasErrors) return;
				var message = ConfigViewModel.GetErrors(args.PropertyName).Cast<string>().First();
				var status = new StatusEvent(StatusCode.Error,
					message);

				var model = InternalMessageModel.Builder().WithType(InternalMessageType.Error).AttachTimeStamp(true)
				   .AttachTextMessage(message).BuildMessage();

				ShowNotification(status);

				AddLog(model);
			};

			ConfigViewModel.PropertyChanged += (sender, args) =>
			{
				if (ConfigViewModel.HasErrors) return;

				var name = args.PropertyName;
			};
		}

		private void ShowNotification(StatusEvent @event) =>
			ManagedNotificationManager.Show(
				NotificationViewModelFactory.Create(@event.StatusCode, @event.StatusMessage));

		private void AddLog(InternalMessageModel log) => Dispatcher.UIThread.InvokeAsync(() => Logs.Add(log));

		[UsedImplicitly]
		public ObservableCollection<InternalMessageModel> Logs { get; } =
			new ObservableCollection<InternalMessageModel>();

		[UsedImplicitly]
		public ObservableCollection<ServerModel> AccessibleServers { get; } = new ObservableCollection<ServerModel>();

		[UsedImplicitly]
		public ObservableCollection<InternalMessageModel> TimeMessages { get; } =
			new ObservableCollection<InternalMessageModel>();
	}
}