namespace NetworkProgramming.Lab4.Models
{
   public class NetworkInterfaceModel
   {
      public string Name { get; set; }
      public string Ip { get; set; }

      public override string ToString()
      {
         return $"{Name} | {Ip}";
      }
   }
}
