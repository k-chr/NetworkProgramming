using System;
using System.Collections.ObjectModel;
using CustomControls.Models;
using ReactiveUI;

namespace UdpServer.ViewModels
{
   public class ConversationViewModel : ViewModelBase
   {
      private ClientModel _client;
      private string _inputMessage;

      public ClientModel Client
      {
         get => _client;
         set => this.RaiseAndSetIfChanged(ref _client, value);
      }

      public string InputMessage
      {
         get => _inputMessage;
         set => this.RaiseAndSetIfChanged(ref _inputMessage, value);
      }

      public ObservableCollection<InternalMessageModel> Messages { get; set; }

      public event EventHandler<Tuple<string, int, string>> SendMessageEvent;

      public void SendMessage()
      {
         SendMessageEvent?.Invoke(this, (InputMessage,Client.Port, Client.Ip ).ToTuple());
         InputMessage = "";
      }
   }
}
