using System;
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
using ReactiveUI;
using UdpNetworking.Services;

namespace UdpServer.ViewModels
{
   public class MainWindowViewModel : ViewModelBase
   {
      private string _ipAddress;
      private string _port;
      private readonly UdpServerService _udpServer;
      private ClientModel _you;
      private IBrush _themeStrongAccentBrush;
      private readonly IBrush _lightThemeBrush;
      private readonly IBrush _darkThemeBrush;
      private int _currentPage;
      private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

      public ObservableCollection<ConversationViewModel> Conversations { get; set; }

      public ObservableCollection<InternalMessageModel> Logs { get; set; }

      public string Version => Assembly.GetAssembly(typeof(MainWindowViewModel)).GetName().Version.ToString();

      public int CurrentPage
      {
         get => _currentPage;
         set => this.RaiseAndSetIfChanged(ref _currentPage, value);
      }

      public MainWindowViewModel()
      {
         _udpServer = new UdpServerService();
         Port = "7";
         _currentPage = 0;
         _lightThemeBrush = Brush.Parse("#ff3030");
         _darkThemeBrush = Brush.Parse("#cd2626");
         IpAddress = "127.0.0.1";
         _themeStrongAccentBrush = _lightThemeBrush;
         Conversations = new ObservableCollection<ConversationViewModel>();
         Logs = new ObservableCollection<InternalMessageModel>();
         _udpServer.NewLog += (sender, objects) => AddLog(objects);
         _udpServer.NewMessage += (sender, objects) => AddMessage(objects);
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

      private void AddMessage(object[] objects)
      {
         var msg = Parse(objects);
         if (msg != null)
         {
            _manualResetEvent.WaitOne();
            var conversation = Conversations.First(c => c.Client.Ip.Equals(msg.ClientModelData.Ip) && c.Client.Port == msg.ClientModelData.Port);
            Dispatcher.UIThread.InvokeAsync(() => conversation.Messages.Add(msg));
            SendMessage(msg.Data, msg.ClientModelData.Ip, msg.ClientModelData.Port);
         }
      }

      private void AddLog(object[] objects)
      {
         var log = Parse(objects);
         if (log != null)
         {
            AddLog(log);
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
            Conversations.FirstOrDefault(c => c.Client.Ip.Equals(ip.Address.ToString()) && c.Client.Port == ip.Port);
         if (conversation == null)
         {
            var model = new ClientModel((ip.Port, ip.Address.ToString()).ToTuple());
            AddClient(model);
            return model;
         }

         return conversation.Client;
      }

      private InternalMessageModel Parse(object[] args)
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

            return builder.AttachTimeStamp(true).BuildMessage();
         }
         catch (Exception e)
         {
            var msg = InternalMessageModel.Builder().AttachExceptionData(e).AttachTimeStamp(true)
               .WithType(InternalMessageType.Error)
               .AttachTextMessage($"Can't parse provided {args} of length {args?.Length ?? 0}").BuildMessage();
            AddLog(msg);
            return null;
         }
      }


      public void SendMessage(string message, string ip, int port)
      {
         if (string.IsNullOrEmpty(message)) return;
         _udpServer.Send(message, ip, port);
         var builder = InternalMessageModel.Builder();
         foreach (var model in Conversations)
         {
            Debug.WriteLine($"{model.Client.Ip} == {ip} = {model.Client.Ip.Equals(ip)}");
            Debug.WriteLine($"{model.Client.Port} == {port} = {model.Client.Port.Equals(port)}");
         }
         var conversation = Conversations.ToList().FirstOrDefault(c => c.Client.Ip.Equals(ip) && c.Client.Port == port);
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
         _you = new ClientModel((port, IpAddress).ToTuple()) { Id = "Server" };
         _udpServer.InitializeTransfer(port, IpAddress);
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
