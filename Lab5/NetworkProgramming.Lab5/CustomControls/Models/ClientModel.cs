using System;

namespace CustomControls.Models
{
   public class ClientModel
   {
      public string Id { get; set; }
      public string Ip { get; }
      public int Port { get; }
      public string Shorten => Id.Substring(0, 1);
      public ClientModel(Tuple<int, string> info)
      {
         Id = $"Client_{Guid.NewGuid()}";
         (Port, Ip) = info;
      }

      public override string ToString()
      {
         return $"[Id]: {Id}\n[Ip]: {Ip}\n[Port]: {Port}\n";
      }

   }
}
