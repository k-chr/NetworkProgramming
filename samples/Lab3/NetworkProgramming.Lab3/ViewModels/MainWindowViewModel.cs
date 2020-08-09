using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Avalonia.Threading;
using CustomControls.Models;
using NetworkingUtilities.Tcp;
using NetworkingUtilities.Utilities.Events;
using ReactiveUI;

namespace NetworkProgramming.Lab3.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public bool MenuVisible
		{
			get => _menuVisible;
			set => this.RaiseAndSetIfChanged(ref _menuVisible, value);
		}

		public bool MainViewVisible
		{
			get => _mainViewVisible;
			set => this.RaiseAndSetIfChanged(ref _mainViewVisible, value);
		}

		public bool ShowPopup
		{
			get => _showPopup;
			set => this.RaiseAndSetIfChanged(ref _showPopup, value);
		}

		public ObservableCollection<ClientModel> Clients { get; set; }
		public ObservableCollection<NetworkInterfaceModel> AvailableInterfaces { get; }
		public ObservableCollection<InternalMessageModel> Logs { get; }

		private IterativeServer _server;
		private bool _menuVisible = true;
		private bool _mainViewVisible = false;
		private bool _showPopup;
		public string Port { get; set; }
		public NetworkInterfaceModel SelectedInterface { get; set; }

		public void OnAcceptCommand()
		{
			_server.AcceptNext();
			ShowPopup = false;
		}

		public void OnRejectCommand()
		{
			OnStopCommand();
			ShowPopup = false;
		}

		public void OnStopCommand()
		{
			Logs.Clear();
			Clients.Clear();
			_server.StopService();
			MenuVisible = true;
			MainViewVisible = false;
		}

		public void OnStartCommand()
		{
			MenuVisible = false;
			MainViewVisible = true;
			var port = int.TryParse(Port ?? "", out var num) ? num : 7;
			_server = new IterativeServer(SelectedInterface?.Ip ?? "127.0.0.1", port,
				SelectedInterface?.Name ?? "localhost");
			RegisterServer();
			_server.StartService();
		}

		private void RegisterServer()
		{
			_server.AddNewClientSubscription((sender, obj) =>
			{
				if (obj is ClientEvent clientEvent)
				{
					var model = new ClientModel((clientEvent.Ip.ToString(), clientEvent.Id, clientEvent.Port));
					var messageModel = InternalMessageModel.Builder().WithType(InternalMessageType.Success)
					   .AttachTimeStamp(true).AttachTextMessage("Successfully accepted new client").BuildMessage();
					AddClient(model);
					AddLog(messageModel);
				}
			});

			_server.AddMessageSubscription((sender, obj) =>
			{
				if (obj is MessageEvent messageEvent)
				{
					var builder = InternalMessageModel.Builder().AttachTextMessage(Encoding.ASCII.GetString(messageEvent.Message));
					builder = builder.WithType(InternalMessageType.Client)
						   .AttachClientData(Clients.First(clientModel => clientModel.Id.Equals(messageEvent.From)));

					var model = builder.BuildMessage();
					AddLog(model);
					_server.Send(messageEvent.Message);
				}
			});

			_server.AddOnDisconnectedSubscription((sender, args) =>
			{
				if (args is ClientEvent @event)
				{
					var messageModel = InternalMessageModel.Builder().WithType(InternalMessageType.Success)
					   .AttachTimeStamp(true)
					   .AttachTextMessage($"Successfully disconnected client: {@event.Id}").BuildMessage();
					AddLog(messageModel);
				}

				ShowPopUp();
			});

			_server.AddStatusSubscription((o, o1) =>
			{
				if (o1 is StatusEvent status)
				{
					var type = status.StatusCode switch
							   {
								   StatusCode.Error => InternalMessageType.Error,
								   StatusCode.Success => InternalMessageType.Success,
								   StatusCode.Info => InternalMessageType.Info,
								   _ => throw new ArgumentOutOfRangeException()
							   };
					var messageModel = InternalMessageModel.Builder().WithType(type).AttachTimeStamp(true)
					   .AttachTextMessage(status.StatusMessage).BuildMessage();

					AddLog(messageModel);
				}
			});

			_server.AddExceptionSubscription((o, o1) =>
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
					var builder = InternalMessageModel.Builder().WithType(InternalMessageType.Error)
					   .AttachExceptionData(exceptionEvent.LastError).AttachTextMessage(message);
					AddLog(builder.BuildMessage());
				}
			});
		}

		public MainWindowViewModel()
		{
			MainViewVisible = false;
			MenuVisible = true;
			Logs = new ObservableCollection<InternalMessageModel>();
			Clients = new ObservableCollection<ClientModel>();

			AvailableInterfaces = new ObservableCollection<NetworkInterfaceModel>(GetNetworkInterfaces());
		}

		private void ShowPopUp()
		{
			ShowPopup = MainViewVisible;
		}

		private void AddClient(ClientModel model)
		{
			Dispatcher.UIThread.InvokeAsync(() => Clients.Add(model));
		}

		private void AddLog(InternalMessageModel model)
		{
			Dispatcher.UIThread.InvokeAsync(() => Logs.Add(model));
		}

		private List<NetworkInterfaceModel> GetNetworkInterfaces()
		{
			var interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(networkInterface =>
				networkInterface.OperationalStatus == OperationalStatus.Up);
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
	}
}