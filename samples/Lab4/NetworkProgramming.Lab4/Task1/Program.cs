using System;
using System.Threading;

namespace Task1
{
   class Program
   {
      static void Main(string[] args)
      {
         Thread th = new Thread(() => Console.WriteLine("Hello World!"));
         th.Start();
         th.Join();
      }
   }
}
