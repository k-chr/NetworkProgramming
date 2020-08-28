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
using TimeProjectServices.ViewModels;
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
		private bool _disabledDiscovery = true;
		private CancellationTokenSource _cancelDiscoverySource = new CancellationTokenSource();
		private int _selectedView;
		private long _tStart;
		private const int Unit = 1000;
		private readonly object _lock = new object();
		private CancellationTokenSource _cancelTimeCommunicationSource = new CancellationTokenSource();
		private readonly LogDumper _dumper;
		private string _currentTime;
		private string _previousTime;
		private long _currentDelta;
		private long _previousDelta;

		[UsedImplicitly] public string DeltaTag => "[Delta]";
		[UsedImplicitly] public string TimeTag => "[Server Time]";
		[UsedImplicitly] public ConfigViewModel ConfigViewModel { get; }

		[UsedImplicitly]
		public IManagedNotificationManager ManagedNotificationManager
		{
			get => _managedNotificationManager;
			set => this.RaiseAndSetIfChanged(ref _managedNotificationManager, value);
		}

		[UsedImplicitly]
		public string CurrentTime
		{
			get => _currentTime;
			set => this.RaiseAndSetIfChanged(ref _currentTime, value);
		}

		[UsedImplicitly]
		public string PreviousTime
		{
			get => _previousTime;
			set => this.RaiseAndSetIfChanged(ref _previousTime, value);
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

		[UsedImplicitly]
		public long CurrentDelta
		{
			get => _currentDelta;
			set => this.RaiseAndSetIfChanged(ref _currentDelta, value);
		}

		[UsedImplicitly]
		public long PreviousDelta
		{
			get => _previousDelta;
			set => this.RaiseAndSetIfChanged(ref _previousDelta, value);
		}

		[UsedImplicitly] public bool CanDiscover => _disabledDiscovery && ConnectedServer == null;

		private void OnConnectedServerChanged(ServerModel old, ServerModel connectedServer)
		{
			if (!(connectedServer is null) && (old is null || !old.Equals(connectedServer)))
				ConfigViewModel.SelectedServer = connectedServer;
		}

		public void OnClosing()
		{
			_client?.StopService();
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				AccessibleServers?.Clear();
				Logs?.Clear();
			});

			_dumper.End();
		}

		private void PrepareTimeMessageTask()
		{
			_cancelTimeCommunicationSource = new CancellationTokenSource();
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
			_cancelDiscoverySource = new CancellationTokenSource();
			timer.Elapsed += (sender, args) =>
			{
				if (!_cancelDiscoverySource.IsCancellationRequested)
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
			_disabledDiscovery = false;
			Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(nameof(CanDiscover)));
		}

		public MainWindowViewModel(IManagedNotificationManager managedNotificationManager,
			ConfigViewModel configViewModel)
		{
			_managedNotificationManager = managedNotificationManager;
			ConfigViewModel = configViewModel;

			ConfigViewModel.ErrorsChanged += OnConfigViewModelOnErrorsChanged;

			ConfigViewModel.ConfigurationChanged += OnConfigViewModelOnConfigurationChanged;
			_dumper = new LogDumper("client_status.log");

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

			AddLog(model, true);
		}

		private void OnConfigViewModelOnConfigurationChanged([CanBeNull] object sender, EventArgs args)
		{
			if (ConfigViewModel.HasErrors) return;
			_cancelDiscoverySource.Cancel(false);
			_cancelTimeCommunicationSource.Cancel(false);
			_disabledDiscovery = true;
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				AccessibleServers.Clear();
				if (ConnectedServer != null)
				{
					SelectedServer = ConnectedServer;
					AccessibleServers.Add(SelectedServer);
					ConnectedServer = null;
				}

				CurrentTime = null;
				PreviousTime = null;
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
			if (o1 is MessageEvent message && !_disabledDiscovery)
			{
				var protocol = ProtocolFactory.FromBytes(message.Message);
				if (protocol == null || protocol.Header != HeaderType.Discover ||
					protocol.Action != ActionType.Response) return;
				if (AccessibleServers.Any(serverModel => serverModel.Ip.Equals(((DiscoverProtocol) protocol).Data))
				) return;
				var model = (((DiscoverProtocol) protocol).Data, $"server_{LocalIdSupplier.CreateId()}");
				AddServer(model);
				var log = InternalMessageModel.Builder().WithType(InternalMessageType.Info).AttachTimeStamp(true)
				   .AttachTextMessage($"Discovered server: {model}").BuildMessage();
				AddLog(log, true);
				ShowNotification(new StatusEvent(StatusCode.Success, $"Discovered server: {model}"));
			}
		}

		private void OnNewDisconnection(object o, object o1)
		{
			if (o1 is ClientEvent)
			{
				ConnectedServer = null;
				_disabledDiscovery = false;
				PrepareDiscoveryTask();
				Dispatcher.UIThread.InvokeAsync(() =>
				{
					this.RaisePropertyChanged(nameof(CanDiscover));
					SelectedView = 1;
					AccessibleServers.Clear();
				});
			}
		}

		private void OnNewConnection(object o, object o1)
		{
			if (o1 is ClientEvent clientEvent)
			{
				_disabledDiscovery = true;
				var endpoint = clientEvent.ServerIp;
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

				AddLog(log, true);

				var status = new StatusEvent(StatusCode.Error, message);
				ShowNotification(status);
			}
		}

		private void OnNewTimeMessage(object o, object o1)
		{
			if (o1 is MessageEvent messageEvent)
			{
				Task.Run(() =>
				{
					var protocol = ProtocolFactory.FromBytes(messageEvent.Message);
					if (protocol != null && protocol.Header == HeaderType.Time &&
						protocol.Action == ActionType.Response)
					{
						var tCli = DateTimeOffset.Now.ToUnixTimeMilliseconds();
						var time = ((TimeProtocol) protocol).Data.ToUnixTimeMilliseconds();
						var delta = time + (tCli - _tStart) / 2 - tCli;
						var serverTime = DateTimeOffset.FromUnixTimeMilliseconds(tCli + delta);
						var str = $"{"[Server Time]",15}\t{serverTime:O}\n{"[Delta]",15}\t{delta,40}[ms]";
						var message = InternalMessageModel.Builder().WithType(InternalMessageType.Server)
						   .AttachTextMessage(str)
						   .AttachTimeStamp(true).AttachClientData(new ClientModel((
								ConnectedServer.Ip.Address.ToString(),
								ConnectedServer.Name, ConnectedServer.Ip.Port))).BuildMessage();
						AddLog(message);
						SetTimeMessage($"{serverTime:O}", delta);
						_event.Set();
					}
				});
			}
		}

		private void SetTimeMessage(string message, long delta) =>
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				PreviousDelta = CurrentDelta;
				PreviousTime = CurrentTime;
				CurrentTime = message;
				CurrentDelta = delta;
			});

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
			}
		}

		[UsedImplicitly]
		public void ConnectToSelectedServer()
		{
			if (SelectedServer != null)
			{
				_client?.StartTimeCommunication(SelectedServer.Ip);
				_disabledDiscovery = true;
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

		private void ShowNotification(StatusEvent @event) => Dispatcher.UIThread.InvokeAsync(() =>
			ManagedNotificationManager.Show(
				NotificationViewModelFactory.Create(@event.StatusCode, @event.StatusMessage)));

		private void AddServer((IPEndPoint Data, string) model) =>
			Dispatcher.UIThread.InvokeAsync(() => AccessibleServers.Add(model));

		private void AddLog(InternalMessageModel log, bool addLogToView = false)
		{
			_dumper.DumpLog(log);
			if (addLogToView)
				Dispatcher.UIThread.InvokeAsync(() => Logs.Add(log));
		}

		[UsedImplicitly]
		public ObservableCollection<InternalMessageModel> Logs { get; } =
			new ObservableCollection<InternalMessageModel>();

		[UsedImplicitly]
		public ObservableCollection<ServerModel> AccessibleServers { get; } = new ObservableCollection<ServerModel>();
	}
}