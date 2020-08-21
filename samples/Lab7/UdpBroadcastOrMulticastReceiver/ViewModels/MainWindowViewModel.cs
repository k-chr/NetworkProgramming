using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using Avalonia.Media;
using Avalonia.Threading;
using CustomControls.Models;
using NetworkingUtilities.Udp.Multicast;
using NetworkingUtilities.Utilities.Events;
using ReactiveUI;

namespace UdpBroadcastOrMulticastReceiver.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private readonly string _initPort = "2020";
		private readonly string _initAddress = "224.0.0.3";
		private MulticastBroadcastServer _service;
		private bool _broadcastEnabled;
		private string _multicastAddress;
		private string _port;
		private int _currentIndex;
		private string _mode;
		private readonly List<ClientModel> _clientsList = new List<ClientModel>();

		public ObservableCollection<InternalMessageModel> Logs { get; set; }
		public ObservableCollection<InternalMessageModel> Messages { get; set; }

		public IBrush ThemeStrongAccentBrush => Brush.Parse("#cd2626");

		public string Mode
		{
			get => _mode;
			set => this.RaiseAndSetIfChanged(ref _mode, value);
		}

		public string MulticastAddress
		{
			get => _multicastAddress;
			set => this.RaiseAndSetIfChanged(ref _multicastAddress, value);
		}

		public string Port
		{
			get => _port;
			set => this.RaiseAndSetIfChanged(ref _port, value);
		}

		public bool BroadcastEnabled
		{
			get => _broadcastEnabled;
			set
			{
				this.RaiseAndSetIfChanged(ref _broadcastEnabled, value);
				Mode = "Mode: ";
				Mode += value switch
						{
							true => "Broadcast",
							false => "Multicast",
						};
			}
		}

		public int CurrentIndex
		{
			get => _currentIndex;
			set => this.RaiseAndSetIfChanged(ref _currentIndex, value);
		}

		public MainWindowViewModel()
		{
			BroadcastEnabled = false;
			CurrentIndex = 0;
			Port = _initPort;
			MulticastAddress = _initAddress;
			Logs = new ObservableCollection<InternalMessageModel>();
			Messages = new ObservableCollection<InternalMessageModel>();
		}

		public void ToggleBroadcast(string s)
		{
			BroadcastEnabled = s switch
							   {
								   "0" => true,
								   "1" => false,
								   _ => false
							   };
		}

		public void OnLogOut()
		{
			Clear();
			CurrentIndex = 0;
		}

		private void Clear()
		{
			Port = _initPort;
			MulticastAddress = _initAddress;
			_service?.StopService();
			Logs.Clear();
			Messages.Clear();
		}

		private ClientModel FindClientModel(IPEndPoint ip)
		{
			var client =
				_clientsList.FirstOrDefault(c => c.Ip.Equals(ip.Address.ToString()) && c.Port == ip.Port);
			if (client == null)
			{
				var model = new ClientModel((ip.Address.ToString(), "", ip.Port));
				_clientsList.Add(model);
				return model;
			}

			return client;
		}

		public void OnLogIn()
		{
			CurrentIndex = 1;
			Messages?.Clear();
			Logs?.Clear();
			try
			{
				var port = int.Parse(Port);
				_service = new MulticastBroadcastServer(MulticastAddress, port, "", BroadcastEnabled);
				var builder = InternalMessageModel.Builder();
				RegisterServer(_service);
				_service.StartService();
				builder.AttachTimeStamp(true).WithType(InternalMessageType.Info);
				builder.AttachTextMessage("Prepared " + (BroadcastEnabled ? "broadcast" : "multicast") + " module");
				AddLog(builder.BuildMessage());
			}
			catch (Exception e)
			{
				var msg = InternalMessageModel.Builder().AttachExceptionData(e)
				   .AttachTextMessage("Couldn't parse provided port").AttachTimeStamp(true)
				   .WithType(InternalMessageType.Error).BuildMessage();
				AddLog(msg);
			}
		}

		private void RegisterServer(MulticastBroadcastServer udpServer)
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
					var textMessage = Encoding.ASCII.GetString(message.Message);
					if (message.From.Equals(message.To))
					{
						var builder = InternalMessageModel.Builder().AttachTimeStamp(true)
						   .WithType(InternalMessageType.Info).AttachTextMessage(textMessage);
						AddLog(builder.BuildMessage());
					}
					else
					{
						var client = FindClientModel(IPEndPoint.Parse(message.From));
						var builder = InternalMessageModel.Builder().AttachTimeStamp(true)
						   .WithType(InternalMessageType.Client).AttachTextMessage(textMessage)
						   .AttachClientData(client);
						AddMsg(builder.BuildMessage());
					}
				}
			});
		}

		public void AddLog(InternalMessageModel model)
		{
			Dispatcher.UIThread.InvokeAsync(() => Logs.Add(model));
		}


		private void AddMsg(InternalMessageModel msg)
		{
			Dispatcher.UIThread.InvokeAsync(() => Messages.Add(msg));
		}

		protected override void ExecuteClosing(CancelEventArgs args)
		{
			Clear();
			base.ExecuteClosing(args);
		}
	}
}