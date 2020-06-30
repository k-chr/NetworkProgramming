using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Avalonia.Threading;
using NetworkProgramming.Lab3.Models;
using NetworkProgramming.Lab3.Services;
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

		private readonly IterativeServer _server = new IterativeServer();
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
			_server.StartService(SelectedInterface?.Ip ?? "127.0.0.1", port, SelectedInterface?.Name ?? "localhost");
		}

		public MainWindowViewModel()
		{
			MainViewVisible = false;
			MenuVisible = true;
			Logs = new ObservableCollection<InternalMessageModel>();
			Clients = new ObservableCollection<ClientModel>();
			_server.OnNewClient += (sender, model) => AddClient(model);
			_server.OnLogEvent += (sender, model) => AddLog(model);
			_server.OnDisconnect += (sender, args) => ShowPopUp();
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