using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Avalonia.Media;
using Avalonia.Threading;
using CustomControls.Models;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Udp.Unicast;
using NetworkingUtilities.Utilities.Events;
using ReactiveUI;

namespace UdpServer.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private string _ipAddress;
		private string _port;
		private AbstractServer _udpServer;
		private ClientModel _you;
		private IBrush _themeStrongAccentBrush;
		private readonly IBrush _lightThemeBrush;
		private readonly IBrush _darkThemeBrush;
		private int _currentPage;
		private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

		public ObservableCollection<ConversationViewModel> Conversations { get; set; }

		public ObservableCollection<InternalMessageModel> Logs { get; set; }

		public string Version => Assembly.GetAssembly(typeof(MainWindowViewModel))?.GetName().Version?.ToString();

		public int CurrentPage
		{
			get => _currentPage;
			set => this.RaiseAndSetIfChanged(ref _currentPage, value);
		}

		public MainWindowViewModel()
		{
			Port = "7";
			_currentPage = 0;
			_lightThemeBrush = Brush.Parse("#ff3030");
			_darkThemeBrush = Brush.Parse("#cd2626");
			IpAddress = "127.0.0.1";
			_themeStrongAccentBrush = _lightThemeBrush;
			Conversations = new ObservableCollection<ConversationViewModel>();
			Logs = new ObservableCollection<InternalMessageModel>();
		}

		public string IpAddress
		{
			get => _ipAddress;
			set => this.RaiseAndSetIfChanged(ref _ipAddress, value);
		}

		public IBrush ThemeStrongAccentBrush
		{
			get => _themeStrongAccentBrush;
			set => this.RaiseAndSetIfChanged(ref _themeStrongAccentBrush, value);
		}

		public string Port
		{
			get => _port;
			set => this.RaiseAndSetIfChanged(ref _port, value);
		}

		private void AddMessage(InternalMessageModel msg)
		{
			if (msg != null)
			{
				_manualResetEvent.WaitOne();
				var conversation = Conversations.First(c =>
					c.Client.Ip.Equals(msg.ClientModelData.Ip) && c.Client.Port == msg.ClientModelData.Port);
				Dispatcher.UIThread.InvokeAsync(() => conversation.Messages.Add(msg));
			}
		}

		private void AddLog(InternalMessageModel model)
		{
			Dispatcher.UIThread.InvokeAsync(() => Logs.Add(model));
		}

		private void AddClient(ClientModel obj)
		{
			_manualResetEvent.Reset();
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				var model = new ConversationViewModel
				{
					Client = obj,
					Messages = new ObservableCollection<InternalMessageModel>()
				};
				model.SendMessageEvent += (sender, tuple) => SendMessage(tuple.Item1, tuple.Item3, tuple.Item2);

				Conversations.Add(model);
				_manualResetEvent.Set();
			});
		}

		private ClientModel FindClientModel(IPEndPoint ip)
		{
			var conversation =
				Conversations.FirstOrDefault(c =>
					c.Client.Ip.Equals(ip.Address.ToString()) && c.Client.Port == ip.Port);
			if (conversation == null)
			{
				var model = new ClientModel((ip.Address.ToString(), "", ip.Port));
				AddClient(model);
				return model;
			}

			return conversation.Client;
		}

		public void SendMessage(string message, string ip, int port)
		{
			if (string.IsNullOrEmpty(message)) return;
			_udpServer.Send(message, $"{ip}:{port}");
			var builder = InternalMessageModel.Builder();
			foreach (var model in Conversations)
			{
				Debug.WriteLine($"{model.Client.Ip} == {ip} = {model.Client.Ip.Equals(ip)}");
				Debug.WriteLine($"{model.Client.Port} == {port} = {model.Client.Port.Equals(port)}");
			}

			var conversation = Conversations.ToList()
			   .FirstOrDefault(c => c.Client.Ip.Equals(ip) && c.Client.Port == port);
			var msg = builder.AttachClientData(_you).AttachTextMessage(message).WithType(InternalMessageType.Server)
			   .AttachTimeStamp(true).BuildMessage();
			Dispatcher.UIThread.InvokeAsync(() => conversation?.Messages.Add(msg));
		}

		public void OnLogOut()
		{
			CurrentPage = 0;
			AppViewClear();
		}

		public void OnLogIn()
		{
			CurrentPage = 1;
			var port = int.Parse(Port);
			_you = new ClientModel((IpAddress, "server", port));
			_udpServer = new UnicastServer(IpAddress, port, "");
			RegisterServer(_udpServer);
			_udpServer.StartService();
		}

		private void RegisterServer(AbstractServer udpServer)
		{
			udpServer.AddExceptionSubscription((o, o1) =>
			{
				if (o1 is ExceptionEvent exceptionEvent)
				{
					var builder = InternalMessageModel.Builder().WithType(InternalMessageType.Error)
					   .AttachExceptionData(exceptionEvent.LastError).AttachTimeStamp(true);
					AddLog(builder.BuildMessage());
				}
			});

			udpServer.AddMessageSubscription((o, o1) =>
			{
				if (o1 is MessageEvent message)
				{
					if (message.From.Equals(message.To))
					{
						var builder = InternalMessageModel.Builder().AttachTimeStamp(true)
						   .WithType(InternalMessageType.Info).AttachTextMessage(message.Message);
						AddLog(builder.BuildMessage());
					}
					else
					{
						var client = FindClientModel(IPEndPoint.Parse(message.From));
						var builder = InternalMessageModel.Builder().AttachTimeStamp(true)
						   .WithType(InternalMessageType.Client).AttachTextMessage(message.Message)
						   .AttachClientData(client);
						AddMessage(builder.BuildMessage());
						SendMessage(message.Message, client.Ip, client.Port);
					}
				}
			});
		}

		private void AppViewClear()
		{
			Conversations.Clear();
			Logs.Clear();
			_udpServer?.StopService();
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

		protected override void ExecuteClosing(CancelEventArgs args)
		{
			AppViewClear();
			base.ExecuteClosing(args);
		}
	}
}