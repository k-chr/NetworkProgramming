using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using Avalonia.Media;
using Avalonia.Threading;
using CustomControls.Models;
using ReactiveUI;
using UdpNetworking.Interfaces;
using UdpNetworking.Services;

namespace UdpBroadcastOrMulticastReceiver.ViewModels
{
   public class MainWindowViewModel : ViewModelBase
   {
      private readonly string _initPort = "2020";
      private readonly string _initAddress = "225.0.0.3";
      private IUdpService _service;
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
            var model = new ClientModel((ip.Port, ip.Address.ToString()).ToTuple());
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
            _service = new UdpMulticastBroadcastReceiver();
            _service.LogEvent += (sender, objects) => ParseEvent(objects);
            var builder = InternalMessageModel.Builder();
            ((UdpMulticastBroadcastReceiver)_service).InitSocket(MulticastAddress, port, BroadcastEnabled);
            builder.AttachTextMessage("Prepared multicast module" +
                                      (BroadcastEnabled ? " with broadcast functionality" : ""));

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
                  IPEndPoint m => builder.AttachClientData(FindClientModel(m)),
                  Exception e => builder.AttachExceptionData(e),
                  string s => builder.AttachTextMessage(s),
                  int num => builder.WithType((InternalMessageType)num),
                  _ => throw new ArgumentException("Unrecognized data received from client handler")
               };
            }

            var msg = builder.AttachTimeStamp(true).BuildMessage();
            if (msg.Type == InternalMessageType.Client)
               AddMsg(msg);
            else
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
