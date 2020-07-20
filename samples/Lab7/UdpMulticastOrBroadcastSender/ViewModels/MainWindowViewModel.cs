using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Avalonia.Media;
using Avalonia.Threading;
using CustomControls.Models;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Udp.Multicast;
using NetworkingUtilities.Utilities.Events;
using ReactiveUI;

namespace UdpMulticastOrBroadcastSender.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private readonly string _initPort = "2020";
		private readonly string _initAddress = "224.0.0.3";
		private AbstractClient _service;
		private bool _broadcastEnabled;
		private string _multicastAddress;
		private string _port;
		private string _messageToSend;
		private int _currentIndex;
		private string _mode;

		public ObservableCollection<InternalMessageModel> Logs { get; set; }
		public ObservableCollection<NetworkInterfaceModel> AvailableInterfaces { get; }
		public NetworkInterfaceModel SelectedInterface { get; set; }

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

		public string MessageToSend
		{
			get => _messageToSend;
			set => this.RaiseAndSetIfChanged(ref _messageToSend, value);
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
			AvailableInterfaces = new ObservableCollection<NetworkInterfaceModel>(GetNetworkInterfaces());
			SelectedInterface = AvailableInterfaces[0];
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

		public void Send()
		{
			if (string.IsNullOrEmpty(MessageToSend)) return;
			_service.Send(MessageToSend);
			MessageToSend = "";
		}

		public void OnLogOut()
		{
			Clear();
			CurrentIndex = 0;
		}

		private void Clear()
		{
			MessageToSend = "";
			Port = _initPort;
			MulticastAddress = _initAddress;
			_service?.StopService();
			Logs.Clear();
		}

		public void OnLogIn()
		{
			CurrentIndex = 1;

			try
			{
				var port = int.Parse(Port);
				_service = BroadcastEnabled
					? (AbstractClient) new BroadcastClient(SelectedInterface.Ip, port)
					: new MulticastClient(MulticastAddress, port);

				RegisterService(_service);
				_service.StartService();
				var builder = InternalMessageModel.Builder();

				if (BroadcastEnabled)
				{
					builder.AttachTextMessage("Prepared broadcast module");
				}
				else
				{
					builder.AttachTextMessage("Prepared multicast module");
				}

				builder.AttachTimeStamp(true).WithType(InternalMessageType.Info);

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

		private void RegisterService(AbstractClient service)
		{
			service.AddExceptionSubscription((o, o1) =>
			{
				if (o1 is ExceptionEvent e)
				{
					var builder = InternalMessageModel.Builder().AttachTimeStamp(true)
					   .WithType(InternalMessageType.Error).AttachExceptionData(e.LastError);
					AddLog(builder.BuildMessage());
				}
			});

			service.AddOnDisconnectedSubscription((o, o1) =>
			{
				var builder = InternalMessageModel.Builder().WithType(InternalMessageType.Info)
				   .AttachTextMessage("Client has stopped its service");
				AddLog(builder.BuildMessage());
			});
		}

		public void AddLog(InternalMessageModel model)
		{
			Dispatcher.UIThread.InvokeAsync(() => Logs.Add(model));
		}

		private static List<NetworkInterfaceModel> GetNetworkInterfaces()
		{
			var interfaces = NetworkInterface.GetAllNetworkInterfaces();
			var output = new List<NetworkInterfaceModel>();

			foreach (var @interface in interfaces)
			{
				var name = @interface.Name;
				var ip = @interface.GetIPProperties().UnicastAddresses.SingleOrDefault(ipAddressInformation =>
					ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
				output.Add(new NetworkInterfaceModel {Ip = ip?.ToString() ?? "localhost", Name = name});
			}

			return output;
		}

		protected override void ExecuteClosing(CancelEventArgs args)
		{
			Clear();
			base.ExecuteClosing(args);
		}
	}
}