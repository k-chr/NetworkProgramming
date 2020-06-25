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
using ReactiveUI;
using UdpNetworking.Interfaces;
using UdpNetworking.Services;

namespace UdpMulticastOrBroadcastSender.ViewModels
{
   public class MainWindowViewModel : ViewModelBase
   {
      private readonly string _initPort = "2020";
      private readonly string _initAddress = "225.0.0.3";
      private IUdpService _service;
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
         if(string.IsNullOrEmpty(MessageToSend)) return;
         ((IUdpSender)_service).Send(MessageToSend);
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
            _service = BroadcastEnabled ? (IUdpService)new UdpBroadcastClient() : new UdpMulticastClient();
            _service.LogEvent += (sender, objects) => ParseEvent(objects);
            var builder = InternalMessageModel.Builder();

            if (BroadcastEnabled)
            {
               ((UdpBroadcastClient)_service).InitSocket(SelectedInterface.Ip,port);
               builder.AttachTextMessage("Prepared broadcast module");
            }
            else
            {
               ((UdpMulticastClient)_service).InitSocket(MulticastAddress, port);
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

      public void AddLog(InternalMessageModel model)
      {
         Dispatcher.UIThread.InvokeAsync(() => Logs.Add(model));
      }

      private void ParseEvent(object[] args)
      {
         var builder = InternalMessageModel.Builder();
         try
         {
            foreach (var arg in args)
            {
               builder = arg switch
               {
                  ClientModel m => builder.AttachClientData(m),
                  Exception e => builder.AttachExceptionData(e),
                  string s => builder.AttachTextMessage(s),
                  int num => builder.WithType((InternalMessageType)num),
                  _ => throw new ArgumentException("Unrecognized data received from client handler")
               };
            }

            var msg = builder.AttachTimeStamp(true).BuildMessage();
            AddLog(msg);
         }
         catch (Exception e)
         {
            var msg = InternalMessageModel.Builder().AttachExceptionData(e).AttachTimeStamp(true)
               .WithType(InternalMessageType.Error)
               .AttachTextMessage($"Can't parse provided {args} of length {args?.Length ?? 0}").BuildMessage();
            AddLog(msg);
         }
      }


      private static List<NetworkInterfaceModel> GetNetworkInterfaces()
      {
         var interfaces = NetworkInterface.GetAllNetworkInterfaces();
         var output = new List<NetworkInterfaceModel>();

         foreach (var @interface in interfaces)
         {
            var name = @interface.Name;
            var ip = @interface.GetIPProperties().UnicastAddresses.SingleOrDefault(ipAddressInformation => ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
            output.Add(new NetworkInterfaceModel { Ip = ip?.ToString() ?? "localhost", Name = name });
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
