using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Avalonia.Threading;
using CustomControls.Models;
using NetworkingUtilities.Tcp;
using NetworkingUtilities.Utilities.Events;
using NetworkProgramming.Lab4.ViewModels;
using ReactiveUI;

namespace NetworkProgramming.Lab5.ViewModels
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

		public ObservableCollection<ClientModel> Clients { get; set; }
		public ObservableCollection<NetworkInterfaceModel> AvailableInterfaces { get; }
		public ObservableCollection<InternalMessageModel> Logs { get; }

		private MultithreadingServer _server;
		private bool _menuVisible = true;
		private bool _mainViewVisible;
		public string Port { get; set; }
		public string CountOfClients { get; set; }
		public NetworkInterfaceModel SelectedInterface { get; set; }

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
			var count = int.TryParse(CountOfClients ?? "", out var clients) ? clients : 3;
			_server = new MultithreadingServer(SelectedInterface?.Ip ?? "127.0.0.1", port,
				SelectedInterface?.Name ?? "localhost",
				count);
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
					var builder = InternalMessageModel.Builder().AttachTextMessage(messageEvent.Message);
					if (messageEvent.From.Equals(messageEvent.To))
					{
						builder = builder.WithType(InternalMessageType.Info);
					}
					else
					{
						builder = builder.WithType(InternalMessageType.Client)
						   .AttachClientData(Clients.First(clientModel => clientModel.Id.Equals(messageEvent.From)));
					}

					var model = builder.BuildMessage();
					AddLog(model);
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

		private void UpdateClientStatus(string id, string status) => Dispatcher.UIThread.InvokeAsync(() =>
			Clients?.Where(clientModel => clientModel.Id.Equals(id)).ToList()
			   .ForEach(clientModel => clientModel.Connected = status));

		private void AddClient(ClientModel model)
		{
			Dispatcher.UIThread.InvokeAsync(() => Clients.Add(model));
		}

		private void AddLog(InternalMessageModel model)
		{
			Dispatcher.UIThread.InvokeAsync(() => Logs.Add(model));
		}

		protected override void ExecuteClosing(CancelEventArgs args)
		{
			_server?.StopService();
			base.ExecuteClosing(args);
		}

		private List<NetworkInterfaceModel> GetNetworkInterfaces()
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
	}
}