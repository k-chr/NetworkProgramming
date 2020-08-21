using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CustomControls.Models;
using JetBrains.Annotations;
using NetworkingUtilities.Utilities.Events;
using ReactiveUI;
using TimeClient.Models;
using TimeProjectServices.Enums;
using TimeProjectServices.Protocols;
using TimeProjectServices.Services;
using Timer = System.Timers.Timer;

namespace TimeClient.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private readonly AutoResetEvent _event = new AutoResetEvent(true);
		private TimeProjectServices.Services.TimeClient _client;
		private ServerModel _selectedServer;
		private IManagedNotificationManager _managedNotificationManager;
		private ServerModel _connectedServer;
		private bool _disableDiscovery;
		private readonly CancellationTokenSource _cancelDiscoverySource = new CancellationTokenSource();
		private int _selectedView;
		private long _tStart;
		private const int Unit = 1000;
		private readonly object _lock = new object();
		private readonly CancellationTokenSource _cancelTimeCommunicationSource = new CancellationTokenSource();

		[UsedImplicitly] public ConfigViewModel ConfigViewModel { get; }

		[UsedImplicitly]
		public IManagedNotificationManager ManagedNotificationManager
		{
			get => _managedNotificationManager;
			set => this.RaiseAndSetIfChanged(ref _managedNotificationManager, value);
		}

		[UsedImplicitly]
		public ServerModel ConnectedServer
		{
			get => _connectedServer;
			set => this.RaiseAndSetIfChanged(ref _connectedServer, value);
		}

		[UsedImplicitly]
		public int SelectedView
		{
			get => _selectedView;
			set => this.RaiseAndSetIfChanged(ref _selectedView, value);
		}

		[UsedImplicitly]
		public ServerModel SelectedServer
		{
			get => _selectedServer;
			set => this.RaiseAndSetIfChanged(ref _selectedServer, value);
		}

		[UsedImplicitly] public bool CanDiscover => !_disableDiscovery && ConnectedServer == null;

		private void OnConnectedServerChanged(ServerModel old, ServerModel connectedServer)
		{
			if (!(connectedServer is null) && !old.Equals(connectedServer))
				ConfigViewModel.SelectedServer = connectedServer;
		}

		public void OnClosing()
		{
			_client?.StopService();
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				AccessibleServers?.Clear();
				TimeMessages?.Clear();
				Logs?.Clear();
			});
		}

		private void PrepareTimeMessageTask()
		{
			Task.Run(() =>
			{
				while (!_cancelTimeCommunicationSource.Token.IsCancellationRequested)
				{
					lock (_lock)
					{
						_tStart = DateTimeOffset.Now.ToUnixTimeMilliseconds();
						_client.SendProtocol(ProtocolFactory.CreateProtocol(ActionType.Query, HeaderType.Time),
							null);
					}

					_event.WaitOne();
					Thread.Sleep(ConfigViewModel.TimeQueryPeriod);
				}
			}, _cancelTimeCommunicationSource.Token);
		}

		private void PrepareDiscoveryTask()
		{
			var timer = new Timer(ConfigViewModel.DiscoveryQueryPeriod * Unit);
			timer.Elapsed += (sender, args) =>
			{
				if (!_disableDiscovery || !_cancelDiscoverySource.IsCancellationRequested)
				{
					Task.Run(() =>
					{
						_client?.SendProtocol(ProtocolFactory.CreateProtocol(ActionType.Query, HeaderType.Discover),
							$"{ConfigViewModel.MulticastAddress}:{ConfigViewModel.MulticastPort}");
					}, _cancelDiscoverySource.Token);
				}
				else
				{
					timer.Close();
				}
			};

			timer.Start();
		}

		[UsedImplicitly]
		public void StartDiscovering()
		{
			_client?.StartService();
			PrepareDiscoveryTask();
		}

		public MainWindowViewModel(IManagedNotificationManager managedNotificationManager,
			ConfigViewModel configViewModel)
		{
			_managedNotificationManager = managedNotificationManager;
			ConfigViewModel = configViewModel;

			ConfigViewModel.ErrorsChanged += OnConfigViewModelOnErrorsChanged;

			ConfigViewModel.ConfigurationChanged += OnConfigViewModelOnConfigurationChanged;

			_client = new TimeProjectServices.Services.TimeClient(ConfigViewModel.MulticastAddress,
				ConfigViewModel.MulticastPort, ConfigViewModel.LocalPort);
			RegisterClient(_client);
		}

		private void OnConfigViewModelOnErrorsChanged([CanBeNull] object sender, DataErrorsChangedEventArgs args)
		{
			if (!ConfigViewModel.HasErrors) return;
			var message = ConfigViewModel.GetErrors(args.PropertyName).Cast<string>().First();
			if (string.IsNullOrEmpty(message)) return;
			var status = new StatusEvent(StatusCode.Error, message);

			var model = InternalMessageModel.Builder()
			   .WithType(InternalMessageType.Error)
			   .AttachTimeStamp(true)
			   .AttachTextMessage(message)
			   .BuildMessage();

			ShowNotification(status);

			AddLog(model);
		}

		private void OnConfigViewModelOnConfigurationChanged([CanBeNull] object sender, EventArgs args)
		{
			if (ConfigViewModel.HasErrors) return;
			_cancelDiscoverySource.Cancel(false);
			_cancelTimeCommunicationSource.Cancel(false);
			_disableDiscovery = false;
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				AccessibleServers.Clear();
				if (ConnectedServer != null)
				{
					SelectedServer = ConnectedServer;
					AccessibleServers.Add(SelectedServer);
					ConnectedServer = null;
				}

				TimeMessages.Clear();
				SelectedView = 1;
				this.RaisePropertyChanged(nameof(CanDiscover));
			});
			_client?.StopService();
			_client = null;
			_client = new TimeProjectServices.Services.TimeClient(ConfigViewModel.MulticastAddress,
				ConfigViewModel.MulticastPort, ConfigViewModel.LocalPort);
			RegisterClient(_client);
		}

		private void RegisterClient(TimeProjectServices.Services.TimeClient client)
		{
			client.AddDiscoveredServerSubscription(OnDiscoveredServer);

			client.AddOnDisconnectedSubscription(OnNewDisconnection);

			client.AddOnConnectedSubscription(OnNewConnection);

			client.AddExceptionSubscription(OnNewException);

			client.AddTimeMessageSubscription(OnNewTimeMessage);

			client.AddStatusSubscription(OnNewStatus);
		}

		private void OnDiscoveredServer(object o, object o1)
		{
			if (o1 is MessageEvent message && !_disableDiscovery)
			{
				var protocol = ProtocolFactory.FromBytes(message.Message);
				if (protocol == null || protocol.Header != HeaderType.Discover ||
					protocol.Action != ActionType.Response) return;
				if (AccessibleServers.Any(serverModel => serverModel.Ip.Equals(((DiscoverProtocol) protocol).Data))
				) return;
				var model = (((DiscoverProtocol) protocol).Data, $"server_{LocalIdSupplier.CreateId()}");
				AddServer(model);
			}
		}

		private void OnNewDisconnection(object o, object o1)
		{
			if (o1 is ClientEvent)
			{
				_disableDiscovery = false;
				Dispatcher.UIThread.InvokeAsync(() =>
				{
					this.RaisePropertyChanged(nameof(CanDiscover));
					TimeMessages.Clear();
					SelectedView = 1;
				});
			}
		}

		private void OnNewConnection(object o, object o1)
		{
			if (o1 is ClientEvent clientEvent)
			{
				_disableDiscovery = true;
				var endpoint = new IPEndPoint(clientEvent.Ip, clientEvent.Port);
				var old = _connectedServer;
				Dispatcher.UIThread.InvokeAsync(() =>
				{
					ConnectedServer = AccessibleServers.First(model => model.Ip.Equals(endpoint));
					OnConnectedServerChanged(old, _connectedServer);
					this.RaisePropertyChanged(nameof(CanDiscover));
				});

				PrepareTimeMessageTask();
			}
		}

		private void OnNewException(object o, object o1)
		{
			if (o1 is ExceptionEvent exceptionEvent)
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

				AddLog(log);

				var status = new StatusEvent(StatusCode.Error, message);
				ShowNotification(status);
			}
		}

		private void OnNewTimeMessage(object o, object o1)
		{
			if (o1 is MessageEvent messageEvent)
			{
				var protocol = ProtocolFactory.FromBytes(messageEvent.Message);
				if (protocol != null && protocol.Header == HeaderType.Time && protocol.Action == ActionType.Response)
				{
					var tCli = DateTimeOffset.Now.ToUnixTimeMilliseconds();
					var time = ((TimeProtocol) protocol).Data.ToUnixTimeMilliseconds();
					var delta = time + (tCli - _tStart) / 2 - tCli;
					var serverTime = DateTimeOffset.FromUnixTimeMilliseconds(tCli + delta);
					var str = $"{"[Server Time]",15}\t{serverTime:O}\n{"[Delta]",15}\t{delta}[ms]";
					var message = InternalMessageModel.Builder().WithType(InternalMessageType.Server)
					   .AttachTextMessage(str)
					   .AttachTimeStamp(true).AttachClientData(new ClientModel((ConnectedServer.Ip.Address.ToString(),
							ConnectedServer.Name, ConnectedServer.Ip.Port))).BuildMessage();
					AddLog(message);
					AddTimeMessage(message);
					ShowNotification(new StatusEvent(StatusCode.Success, $"New TimeMessage!\n{str}"));
					_event.Set();
				}
			}
		}

		private void AddTimeMessage(InternalMessageModel message) =>
			Dispatcher.UIThread.InvokeAsync(() => TimeMessages.Add(message));

		private void OnNewStatus(object o, object o1)
		{
			if (o1 is StatusEvent statusEvent)
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

				if (type != InternalMessageType.Error) ShowNotification(statusEvent);
			}
		}

		[UsedImplicitly]
		public void ConnectToSelectedServer()
		{
			if (SelectedServer != null)
			{
				_client?.StartTimeCommunication(SelectedServer.Ip);
				_disableDiscovery = true;
				this.RaisePropertyChanged(nameof(CanDiscover));
			}
		}

		[UsedImplicitly]
		public void DisconnectFromServer()
		{
			if (ConnectedServer != null)
			{
				_cancelTimeCommunicationSource.Cancel(false);
				_client?.StopTimeCommunication();
			}
		}

		private void ShowNotification(StatusEvent @event) =>
			ManagedNotificationManager.Show(
				NotificationViewModelFactory.Create(@event.StatusCode, @event.StatusMessage));

		private void AddServer((IPEndPoint Data, string) model) =>
			Dispatcher.UIThread.InvokeAsync(() => AccessibleServers.Add(model));

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