using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CustomControls.Models;
using JetBrains.Annotations;
using NetworkingUtilities.Utilities.Events;
using ReactiveUI;
using TimeProjectServices.Enums;
using TimeProjectServices.Protocols;
using TimeProjectServices.Services;
using TimeProjectServices.ViewModels;
using TimeServer.Models;

namespace TimeServer.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private IManagedNotificationManager _mainWindowNotificationArea;
		private TimeProjectServices.Services.TimeServer _timeServer;
		private int _selectedView;
		private bool _serverStarted;
		private readonly List<ClientEvent> _clients = new List<ClientEvent>();
		private readonly LogDumper _dumper;

		[UsedImplicitly] public ConfigViewModel AppState { get; }

		[UsedImplicitly]
		public IManagedNotificationManager ManagedNotificationManager
		{
			get => _mainWindowNotificationArea;
			set => this.RaiseAndSetIfChanged(ref _mainWindowNotificationArea, value);
		}

		[UsedImplicitly]
		public ObservableCollection<InternalMessageModel> Logs { get; } =
			new ObservableCollection<InternalMessageModel>();

		[UsedImplicitly]
		public ObservableCollection<ServerModel> Servers { get; set; } =
			new ObservableCollection<ServerModel>();

		[UsedImplicitly]
		public int SelectedView
		{
			get => _selectedView;
			set => this.RaiseAndSetIfChanged(ref _selectedView, value);
		}

		[UsedImplicitly]
		public bool ServerStarted
		{
			get => _serverStarted;
			set => this.RaiseAndSetIfChanged(ref _serverStarted, value);
		}

		public MainWindowViewModel(IManagedNotificationManager mainWindowNotificationArea, ConfigViewModel appState)
		{
			_mainWindowNotificationArea = mainWindowNotificationArea;
			AppState = appState;

			AppState.ErrorsChanged += AppStateOnErrorsChanged;
			AppState.ConfigurationChanged += AppStateOnConfigurationChanged;

			_timeServer =
				new TimeProjectServices.Services.TimeServer(AppState.MulticastAddress, AppState.MulticastPort);

			_dumper = new LogDumper("server_status.log");

			RegisterServer();
		}

		private void AppStateOnConfigurationChanged([CanBeNull] object sender, EventArgs e)
		{
			if (AppState.HasErrors) return;
			ServerStarted = false;
			Dispatcher.UIThread.InvokeAsync(() => Servers.Clear());
			_timeServer?.StopService();
			_timeServer = null;
			_timeServer =
				new TimeProjectServices.Services.TimeServer(AppState.MulticastAddress, AppState.MulticastPort);

			RegisterServer();
		}

		private void AppStateOnErrorsChanged([CanBeNull] object sender, DataErrorsChangedEventArgs e)
		{
			if (!AppState.HasErrors) return;
			var message = AppState.GetErrors(e.PropertyName).Cast<string>().First();
			if (string.IsNullOrEmpty(message)) return;
			var status = new StatusEvent(StatusCode.Error, message);

			var model = InternalMessageModel.Builder()
			   .WithType(InternalMessageType.Error)
			   .AttachTimeStamp(true)
			   .AttachTextMessage(message)
			   .BuildMessage();

			ShowNotification(status);
			AddLog(model, true);
		}

		private void RegisterServer()
		{
			_timeServer.AddDiscoverRequestSubscription(OnDiscoverRequest);

			_timeServer.AddStatusSubscription(OnStatus);

			_timeServer.AddExceptionSubscription(OnException);

			_timeServer.AddOnDisconnectionSubscription(OnDisconnect);

			_timeServer.AddTimeMessageRequestSubscription(OnTimeMessageRequest);

			_timeServer.AddOnNewClientSubscription(OnNewClient);
		}

		private void OnTimeMessageRequest(object arg1, object arg2)
		{
			if (arg2 is MessageEvent messageEvent)
			{
				Task.Run(() =>
				{
					var client = _clients.FirstOrDefault(c => c.Id.Equals(messageEvent.From));
					if (client == null) return;
					_timeServer.SendProtocol(
						ProtocolFactory.CreateProtocol(ActionType.Response, HeaderType.Time, DateTimeOffset.Now),
						client.ServerIp, client.Id);

					var msg = InternalMessageModel.Builder()
					   .WithType(InternalMessageType.Info)
					   .AttachTimeStamp(true)
					   .AttachTextMessage($"Received \"TimeRequest\" from: {client.Ip}")
					   .BuildMessage();

					AddLog(msg);
				});
			}
		}

		private void OnNewClient(object arg1, object arg2)
		{
			if (arg2 is ClientEvent clientEvent)
			{
				_clients.Add(clientEvent);
				var server = Servers.FirstOrDefault(model => model.Ip.Equals(clientEvent.ServerIp));
				server?.NewClient();
				Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(nameof(Servers)));
				var msg = InternalMessageModel.Builder()
				   .WithType(InternalMessageType.Info)
				   .AttachTimeStamp(true)
				   .AttachTextMessage($"Client: {clientEvent.Id}, {clientEvent.Ip} connected")
				   .BuildMessage();
				AddLog(msg, true);

				ShowNotification(new StatusEvent(StatusCode.Info,
					$"Client: {clientEvent.Id}, {clientEvent.Ip} connected"));
			}
		}

		private void OnDisconnect(object arg1, object arg2)
		{
			if (arg2 is ClientEvent clientEvent)
			{
				_clients.Remove(clientEvent);
				var server = Servers.FirstOrDefault(model => model.Ip.Equals(clientEvent.ServerIp));
				server?.ClientDisconnected();
				Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(nameof(Servers)));
				var msg = InternalMessageModel.Builder()
				   .WithType(InternalMessageType.Info)
				   .AttachTimeStamp(true)
				   .AttachTextMessage($"Client: {clientEvent.Id}, {clientEvent.Ip} disconnected")
				   .BuildMessage();
				AddLog(msg, true);
				ShowNotification(new StatusEvent(StatusCode.Info,
					$"Client: {clientEvent.Id}, {clientEvent.Ip} disconnected"));
			}
		}

		private void OnException(object arg1, object arg2)
		{
			if (arg2 is ExceptionEvent exceptionEvent)
			{
				var message = exceptionEvent.LastErrorCode switch
							  {
								  EventCode.Connect => "Issues with connection",
								  EventCode.Disconnect => "Cannot disconnect properly",
								  EventCode.Send => "Cannot send message to destination",
								  EventCode.Receive => "Cannot obtain message from sender",
								  EventCode.Accept => "Cannot accept client properly",
								  EventCode.Other => "Unknown error occured",
								  EventCode.Bind => "Cannot bind socket to specified address",
								  _ => throw new ArgumentOutOfRangeException()
							  };
				var log = InternalMessageModel.Builder()
				   .WithType(InternalMessageType.Error)
				   .AttachExceptionData(exceptionEvent.LastError)
				   .AttachTextMessage(message)
				   .AttachTimeStamp(true)
				   .BuildMessage();

				AddLog(log, true);

				var status = new StatusEvent(StatusCode.Error, message);
				ShowNotification(status);
			}
		}

		private void OnStatus(object arg1, object arg2)
		{
			if (arg2 is StatusEvent statusEvent)
			{
				var type = statusEvent.StatusCode switch
						   {
							   StatusCode.Error => InternalMessageType.Error,
							   StatusCode.Success => InternalMessageType.Success,
							   StatusCode.Info => InternalMessageType.Info,
							   _ => throw new ArgumentOutOfRangeException()
						   };
				var log = InternalMessageModel.Builder()
				   .WithType(type)
				   .AttachTimeStamp(true)
				   .AttachTextMessage(statusEvent.StatusMessage)
				   .BuildMessage();
				AddLog(log);
			}
		}

		private void OnDiscoverRequest(object arg1, object arg2)
		{
			if (arg2 is MessageEvent messageEvent)
			{
				var ep = IPEndPoint.Parse(messageEvent.To);
				if (ep.AddressFamily != AddressFamily.InterNetworkV6)
				{
					var strings = messageEvent.To.Split(':');
					ep = IPEndPoint.Parse($"[::ffff:{strings[0]}]:{strings[1]}");
				}

				var server = Servers.FirstOrDefault(model =>
					model.Ip.Address.ToString().Contains(ep.Address.ToString()));
				_timeServer.SendProtocol(
					ProtocolFactory.CreateProtocol(ActionType.Response, HeaderType.Discover, server?.Ip
					),
					IPEndPoint.Parse(messageEvent.To), messageEvent.From);

				var msg = InternalMessageModel.Builder()
				   .WithType(InternalMessageType.Info)
				   .AttachTimeStamp(true)
				   .AttachTextMessage($"Received \"DiscoverRequest\" from: {messageEvent.From}")
				   .BuildMessage();

				AddLog(msg);
			}
		}

		private void AddServer(ServerModel server) => Dispatcher.UIThread.InvokeAsync(() => Servers.Add(server));

		private void AddLog(InternalMessageModel log, bool addToApp = false)
		{
			_dumper.DumpLog(log);
			if (addToApp)
				Dispatcher.UIThread.InvokeAsync(() => Logs.Add(log));
		}

		private void ShowNotification(StatusEvent @event) => Dispatcher.UIThread.InvokeAsync(() =>
			ManagedNotificationManager.Show(
				NotificationViewModelFactory.Create(@event.StatusCode, @event.StatusMessage)));

		public void OnClosing()
		{
			_timeServer?.StopService();
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				Servers.Clear();
				Logs.Clear();
			});

			_dumper.End();
		}

		[UsedImplicitly]
		public void StartServer()
		{
			_timeServer.StartService();
			foreach (var ipEndPoint in _timeServer.WorkingServers)
			{
				AddServer((ipEndPoint, $"server_{LocalIdSupplier.CreateId()}"));
			}

			ServerStarted = true;
		}
	}
}