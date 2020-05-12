using System;
using NetworkingUtilities.Http.Services;

namespace HttpStorehouse
{
   class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("Hello World!");
         WebServer.Builder().UseAsyncInvocations(false).WithPort(8080).WithPrefix(@"http://localhost").Build().StartService();
         Console.ReadKey();
      }
   }
}
