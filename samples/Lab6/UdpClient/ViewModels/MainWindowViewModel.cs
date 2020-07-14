using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Avalonia.Media;
using Avalonia.Threading;
using CustomControls.Models;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Udp.Unicast;
using NetworkingUtilities.Utilities.Events;
using ReactiveUI;

namespace UdpClient.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private int _currentPage;
		private string _port;
		private string _ipAddress;
		private AbstractClient _clientService;
		private IBrush _themeStrongAccentBrush;
		private readonly IBrush _lightThemeBrush;
		private readonly IBrush _darkThemeBrush;
		private readonly ClientModel _you = new ClientModel(("Any", "You", 0));
		private string _inputMessage;
		private ClientModel _server;

		public MainWindowViewModel()
		{
			Port = "7";
			_currentPage = 0;
			_lightThemeBrush = Brush.Parse("#ff3030");
			_darkThemeBrush = Brush.Parse("#cd2626");
			IpAddress = "127.0.0.1";
			Logs = new ObservableCollection<InternalMessageModel>();
			Messages = new ObservableCollection<InternalMessageModel>();
			_themeStrongAccentBrush = _lightThemeBrush;
		}

		public void SendMessage()
		{
			if (string.IsNullOrEmpty(InputMessage)) return;
			var msg = InternalMessageModel.Builder().WithType(InternalMessageType.Client)
			   .AttachTextMessage(InputMessage)
			   .AttachTimeStamp(true).AttachClientData(_you).BuildMessage();
			AddMessage(msg);
			_clientService.Send(InputMessage);
			InputMessage = "";
		}

		private void AddMessage(InternalMessageModel model)
		{
			Dispatcher.UIThread.InvokeAsync(() => Messages.Add(model));
		}

		private void AddLog(InternalMessageModel model)
		{
			Dispatcher.UIThread.InvokeAsync(() => Logs.Add(model));
		}

		public ObservableCollection<InternalMessageModel> Messages { get; set; }
		public ObservableCollection<InternalMessageModel> Logs { get; set; }

		public string Version => Assembly.GetAssembly(typeof(MainWindowViewModel))?.GetName().Version?.ToString();


		public IBrush ThemeStrongAccentBrush
		{
			get => _themeStrongAccentBrush;
			set => this.RaiseAndSetIfChanged(ref _themeStrongAccentBrush, value);
		}

		public string InputMessage
		{
			get => _inputMessage;
			set => this.RaiseAndSetIfChanged(ref _inputMessage, value);
		}

		public int CurrentPage
		{
			get => _currentPage;
			set => this.RaiseAndSetIfChanged(ref _currentPage, value);
		}

		public string Port
		{
			get => _port;
			set => this.RaiseAndSetIfChanged(ref _port, value);
		}

		public string IpAddress
		{
			get => _ipAddress;
			set => this.RaiseAndSetIfChanged(ref _ipAddress, value);
		}

		protected override void ExecuteClosing(CancelEventArgs args)
		{
			AppViewClear();
			base.ExecuteClosing(args);
		}

		private void AppViewClear()
		{
			Messages.Clear();
			Logs.Clear();
			_clientService?.StopService();
		}

		public void OnLogIn()
		{
			CurrentPage = 1;
			var port = int.Parse(Port);
			_server = new ClientModel((IpAddress, "server", port));
			_clientService = new UnicastClient(IpAddress, port);
			RegisterClient();
			_clientService.StartService();
		}

		private void RegisterClient()
		{
			_clientService.AddExceptionSubscription((o, o1) =>
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
									  _ => throw new ArgumentOutOfRangeException()
								  };
					var builder = InternalMessageModel.Builder().WithType(InternalMessageType.Error)
					   .AttachTimeStamp(true)
					   .AttachExceptionData(exceptionEvent.LastError).AttachTextMessage(message);
					AddLog(builder.BuildMessage());
				}
			});

			_clientService.AddMessageSubscription((o, o1) =>
			{
				if (o1 is MessageEvent messageEvent)
				{
					var builder = InternalMessageModel.Builder().AttachTextMessage(messageEvent.Message);
					if (messageEvent.From.Equals(messageEvent.To))
					{
						builder = builder.WithType(InternalMessageType.Info);
						var model = builder.BuildMessage();
						AddLog(model);
					}
					else if (!messageEvent.From.Equals("server"))
					{
						builder = builder.WithType(InternalMessageType.Client).AttachTimeStamp(true)
						   .AttachClientData(_you);
						var model = builder.BuildMessage();
						AddLog(model);
						AddMessage(model);
					}
					else
					{
						builder = builder.WithType(InternalMessageType.Server).AttachTimeStamp(true)
						   .AttachClientData(_server);
						var model = builder.BuildMessage();
						AddLog(model);
						AddMessage(model);
					}
				}
			});
		}

		public void OnLogOut()
		{
			CurrentPage = 0;
			AppViewClear();
		}

		public void OnStyleChange(string state)
		{
			var s = int.Parse(state);
			ThemeStrongAccentBrush = s switch
									 {
										 1 => _lightThemeBrush,
										 0 => _darkThemeBrush,
										 _ => _lightThemeBrush
									 };
		}
	}
}