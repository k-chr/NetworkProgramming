using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI;
using UdpClient.Models;
using UdpClient.Services;

namespace UdpClient.ViewModels
{
   public class MainWindowViewModel : ViewModelBase
   {
      private int _currentPage;
      private string _port;
      private string _ipAddress;
      private readonly UdpClientService _clientService;
      private IBrush _themeStrongAccentBrush;
      private readonly IBrush _lightThemeBrush;
      private readonly IBrush _darkThemeBrush;
      private readonly ClientModel _you = new ClientModel((0, "Any").ToTuple()){Id = "You"};
      private string _inputMessage;
      private ClientModel _server;

      public MainWindowViewModel()
      {
         Port = "7";
         _currentPage = 0;
         _lightThemeBrush = Brush.Parse("#ff3030");
         _darkThemeBrush = Brush.Parse("#cd2626");
         IpAddress = "127.0.0.1";
         _clientService = new UdpClientService();
         Logs = new ObservableCollection<InternalMessageModel>();
         Messages = new ObservableCollection<InternalMessageModel>();
         _themeStrongAccentBrush = _lightThemeBrush;
         _clientService.NewLog += (sender, objects) => AddLog(objects);
         _clientService.NewMessage += (sender, objects) => AddMessage(objects);
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
                  ClientModel m => builder.AttachClientData(m),
                  Exception e => builder.AttachExceptionData(e),
                  string s => builder.AttachTextMessage(s),
                  int num => builder.WithType((InternalMessageType)num),
                  _ => throw new ArgumentException("Unrecognized data received from client handler")
               };
            }
            
            
            return builder.AttachClientData(_server??new ClientModel((0, "Any").ToTuple())).AttachTimeStamp(true).BuildMessage();
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

      private void AddMessage(object[] objects)
      {
         var msg = Parse(objects);
         if (msg != null)
         {
            AddMessage(msg);
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

      public void SendMessage()
      {
         if(string.IsNullOrEmpty(InputMessage)) return;
         var msg = InternalMessageModel.Builder().WithType(InternalMessageType.Client).AttachTextMessage(InputMessage)
            .AttachTimeStamp(true).AttachClientData(_you).BuildMessage();
         AddMessage(msg);
         _clientService.Send(InputMessage);
         InputMessage = "";
      }

      private void AddMessage(InternalMessageModel model)
      {
         Dispatcher.UIThread.InvokeAsync(() => Messages.Add(model));
      }

      private void AddLog(InternalMessageModel model)
      {
         Dispatcher.UIThread.InvokeAsync(() => Logs.Add(model));
      }

      public ObservableCollection<InternalMessageModel> Messages { get; set; }
      public ObservableCollection<InternalMessageModel> Logs { get; set; }

      public ClientModel TempClient => new ClientModel((8, "localhost").ToTuple());

      public string Version => Assembly.GetAssembly(typeof(MainWindowViewModel)).GetName().Version.ToString();


      public IBrush ThemeStrongAccentBrush
      {
         get => _themeStrongAccentBrush;
         set => this.RaiseAndSetIfChanged(ref _themeStrongAccentBrush, value);
      }

      public string InputMessage
      {
         get => _inputMessage;
         set => this.RaiseAndSetIfChanged(ref _inputMessage, value);
      }

      public int CurrentPage
      {
         get => _currentPage;
         set => this.RaiseAndSetIfChanged(ref _currentPage, value);
      }

      public string Port
      {
         get => _port;
         set => this.RaiseAndSetIfChanged(ref _port, value);
      }

      public string IpAddress
      {
         get => _ipAddress;
         set => this.RaiseAndSetIfChanged(ref _ipAddress, value);
      }

      protected override void ExecuteClosing(CancelEventArgs args)
      {
         AppViewClear();
         base.ExecuteClosing(args);
      }

      private void AppViewClear()
      {
         Messages.Clear();
         Logs.Clear();
         _clientService?.StopService();
      }

      public void OnLogIn()
      {
         CurrentPage = 1;
         var port = int.Parse(Port);
         _server = new ClientModel((port, IpAddress).ToTuple()){Id = "Server"};
         _clientService.InitializeTransfer(port, IpAddress);
      }

      public void OnLogOut()
      {
         CurrentPage = 0;
         AppViewClear();
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
   }
}
