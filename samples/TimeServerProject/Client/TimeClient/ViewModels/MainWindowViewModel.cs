using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
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
		private readonly IManagedNotificationManager _managedNotificationManager;

		private TimeProjectServices.Services.TimeClient _client;

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

		private ReactiveCommand<StatusEvent, Unit> ShowNotification { get; }

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

			ShowNotification = ReactiveCommand.Create<StatusEvent, Unit>(@event =>
			{
				_managedNotificationManager.Show(
					NotificationViewModelFactory.Create(@event.StatusCode, @event.StatusMessage));
				return Unit.Default;
			});

			ConfigViewModel.ErrorsChanged += (sender, args) =>
			{
				var message = ConfigViewModel.GetErrors(args.PropertyName).Cast<string>().First();
				var status = new StatusEvent(StatusCode.Error,
					message);
				ShowNotification.Execute(status);

				var model = InternalMessageModel.Builder().WithType(InternalMessageType.Error).AttachTimeStamp(true)
				   .AttachTextMessage(message).BuildMessage();

				AddLog(model);
			};
		}

		private void AddLog(InternalMessageModel log) => Dispatcher.UIThread.InvokeAsync(() => Logs.Add(log));

		[UsedImplicitly]
		public ObservableCollection<InternalMessageModel> Logs { get; } =
			new ObservableCollection<InternalMessageModel>();

		[UsedImplicitly]
		public ObservableCollection<ServerModel> AccessibleServers { get; } = new ObservableCollection<ServerModel>();
	}
}