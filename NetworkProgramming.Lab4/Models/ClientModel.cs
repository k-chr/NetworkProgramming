using System;
using ReactiveUI;

namespace NetworkProgramming.Lab4.Models
{
   public class ClientModel : ReactiveObject
   {
      private string _connected;
      public string Id { get; }
      public string Ip { get; }
      public int Port { get; }

      public string Connected
      {
         get => _connected;
         set => _connected = this.RaiseAndSetIfChanged(ref _connected, value);
      }

      public ClientModel(Tuple<int, string> info)
      {
         Id = $"Client_{Guid.NewGuid()}";
         (Port, Ip) = info;
      }

      public override bool Equals(object obj)
      {
         return (obj is ClientModel model) && model.Id.Equals(Id);
      }


      public override int GetHashCode()
      {
         return (Id != null ? Id.GetHashCode() : 0);
      }

      public override string ToString()
      {
         return $"Id: {Id} | Ip: {Ip} | Port: {Port}";
      }
   }
}