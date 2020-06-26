using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Avalonia.Threading;
using NetworkProgramming.Lab4.Models;
using NetworkProgramming.Lab4.ViewModels;
using NetworkProgramming.Lab5.Services;
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

		private readonly MultithreadingServer _server = new MultithreadingServer();
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
			_server.StartService(SelectedInterface?.Ip ?? "127.0.0.1", port, SelectedInterface?.Name ?? "localhost",
				count);
		}

		public MainWindowViewModel()
		{
			MainViewVisible = false;
			MenuVisible = true;
			Logs = new ObservableCollection<InternalMessageModel>();
			Clients = new ObservableCollection<ClientModel>();
			_server.OnNewClient += (sender, model) => AddClient(model);
			_server.OnLogEvent += (sender, model) => AddLog(model);
			_server.OnDisconnect += (sender, model) => UpdateClientStatus(model);
			AvailableInterfaces = new ObservableCollection<NetworkInterfaceModel>(GetNetworkInterfaces());
		}

		private void UpdateClientStatus(ClientModel model) => Dispatcher.UIThread.InvokeAsync(() =>
			Clients?.Where(clientModel => clientModel.Equals(model)).ToList()
			   .ForEach(clientModel => clientModel.Connected = model.Connected));

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