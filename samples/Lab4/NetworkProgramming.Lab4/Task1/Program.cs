using System;
using System.Threading;

namespace Task1
{
	static class Program
	{
		static void Main()
		{
			var th = new Thread(() => Console.WriteLine("Hello World!"));
			th.Start();
			th.Join();
		}
	}
}