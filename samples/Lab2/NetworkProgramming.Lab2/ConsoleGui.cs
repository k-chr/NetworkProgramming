using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetworkProgramming.Lab2
{
   public sealed class ConsoleGui
   {
      private static readonly Dictionary<string, Tuple<int, int>> ConsoleLinePositions = new Dictionary<string, Tuple<int, int>>()
      {
         {"INPUT", Tuple.Create(1, 8)},
         {"Info", Tuple.Create(10, 11)},
         {"LOG", Tuple.Create(12, -1)},
      };

      private static readonly Dictionary<int, string> Menu = new Dictionary<int, string>()
      {
         {1, "Connect" },
         {2, "Disconnect" },
         {3, "Send" },
         {4, "Quit" },
      };
      
      private static readonly ManualResetEvent ClientEventDone = new ManualResetEvent(false);
      private static readonly ManualResetEvent Done = new ManualResetEvent(false);
      private static Client _client;

      private static void ClearInputArea()
      {
         Done.Reset();
         var (inputBegin, inputEnd) = ConsoleLinePositions["INPUT"];

         for (var i = inputBegin; i <= inputEnd; ++i)
         {
            Console.SetCursorPosition(0, i);
            Console.Write(new string(' ', Console.WindowWidth));
         }

         Console.SetCursorPosition(0, inputBegin);
      }

      private static void DisplayMenu()
      {
         ClearInputArea();
         var (begin, _) = ConsoleLinePositions["INPUT"];
         Console.SetCursorPosition(0, begin);
         Console.ForegroundColor = ConsoleColor.DarkYellow;
         Console.Write("Type number of operation:");
         ++begin;
         Console.SetCursorPosition(0, begin);
         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.Write("__________________");
         ++begin;

         foreach (var (key, value) in Menu)
         {
            Console.SetCursorPosition(0, begin);
            Console.Write($"| {key} | {value,10} |");
            ++begin;
         }

         Console.SetCursorPosition(0, begin);
         Console.Write("------------------");
         ++begin;
         Console.SetCursorPosition(0, begin);
         Console.ForegroundColor = ConsoleColor.White;
         Console.SetCursorPosition(0, ConsoleLinePositions["Info"].Item1 - 1);
         Console.Write(new string(' ', Console.WindowWidth));
         Console.SetCursorPosition(0, ConsoleLinePositions["Info"].Item1);
         Console.Write("Log: ");
         Console.SetCursorPosition(0, begin);
         Done.Set();
      }

      private static string DisplayMenuFeed()
      {
         DisplayMenu();
         var str = Console.ReadLine();
         return str;
      }

      private static Tuple<string, string> DisplayIPv4PortDialog()
      {
         ClearInputArea();
         var (begin, _) = ConsoleLinePositions["INPUT"];
         Console.SetCursorPosition(0, begin);
         Console.ForegroundColor = ConsoleColor.DarkYellow;
         Console.Write("Provide IPv4 address and port to connect to remote server");
         ++begin;
         Console.SetCursorPosition(0, begin);
         Console.Write("IPv4: ");
         Console.ForegroundColor = ConsoleColor.White;
         var address = Console.ReadLine();
         ++begin;
         Console.SetCursorPosition(0, begin);
         Console.ForegroundColor = ConsoleColor.DarkYellow;
         Console.Write("Port: ");
         Console.ForegroundColor = ConsoleColor.White;
         var portStr = Console.ReadLine();
         ++begin;
         Console.SetCursorPosition(0, begin);
         Done.Set();
         return Tuple.Create(address, portStr);
      }

      private static string DisplayMessageDialog()
      {
         ClearInputArea();
         var (begin, _) = ConsoleLinePositions["INPUT"];
         Console.SetCursorPosition(0, begin);
         Console.ForegroundColor = ConsoleColor.DarkYellow;
         Console.Write("Provide message content to send");
         ++begin;
         Console.SetCursorPosition(0, begin);
         Console.Write("Message: ");
         Console.ForegroundColor = ConsoleColor.White;
         var message = Console.ReadLine();
         ++begin;
         Console.SetCursorPosition(0, begin);
         Done.Set();
         return message;
      }

      public static void Launch(string[] args)
      {
         Init();
         Console.WriteLine("Hello in console application!");
         Logger.LoggerBeginLine = ConsoleLinePositions["LOG"].Item1;
         Logger.ReturnLine = ConsoleLinePositions["INPUT"].Item2;
         Logger.CanWrite = Done;
         var numStr = DisplayMenuFeed();
         while (true)
         {
            try
            {
               var num = int.Parse(numStr);
               if (num < 1 || num > 4)
               {
                  throw new ArgumentException("Invalid parameter, index out of range");
               }

               ProcessEvent(Menu[num]);
            }
            catch (Exception e)
            {
               Logger.LogError(e, "Wrong number of operation or operation is not permitted\n");
            }
            finally{numStr = DisplayMenuFeed();}
         }
      }

      private static void Init()
      {
         var MF_BYCOMMAND = 0x00000000;
         var SC_CLOSE = 0xF060;
         var SC_MINIMIZE = 0xF020;
         var SC_MAXIMIZE = 0xF030;
         var SC_SIZE = 0xF000;
         var handle = GetConsoleWindow(); 
         var sysMenu = GetSystemMenu(handle, false);

         if (handle != IntPtr.Zero)
         {
            DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND);
            DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
         }

         Logger.QuitRequest += (sender, args) =>
         {
            Console.ReadKey();
            Environment.Exit(1);
         };
      }

      [DllImport("user32.dll")]
      private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

      [DllImport("user32.dll")]
      private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

      [DllImport("kernel32.dll", ExactSpelling = true)]
      private static extern IntPtr GetConsoleWindow();

      private static void ProcessEvent(string s)
      {
         switch (s)
         {
            case "Connect":
               ConnectProcedure();
               break;
            case "Disconnect":
               DisconnectProcedure();
               break;
            case "Send":
               SendProcedure();
               break;
            case "Quit":
               QuitProcedure();
               break;
         }
      }

      private static void QuitProcedure()
      {
         if (_client != null && !_client.IsConnected())
         {
            _client?.Disconnect();
         }

         Logger.LogInfo("Shutting down");
         Console.SetCursorPosition(0, Logger.LoggerBeginLine);
         Console.ReadKey();
         Environment.Exit(0);
      }

      private static void SendProcedure()
      {
         if (_client == null || !_client.IsConnected())
         {
            throw new InvalidOperationException("Can't send any data - client is currently not connected with remote host");
         }

         var message = DisplayMessageDialog();

         _client.Send(message);
      }

      private static void DisconnectProcedure()
      {
         if (_client == null || !_client.IsConnected())
         {
            throw new InvalidOperationException("Can't disconnect client due to not being currently connected");
         }

         _client.Disconnect();
      }

      private static void ConnectProcedure()
      {
         if (_client != null && _client.IsConnected())
         {
            throw new InvalidOperationException("Can't connect to server - client is currently connected with remote host");
         }

         var (address, portStr) = DisplayIPv4PortDialog();

         try
         {
            var port = int.Parse(portStr);
            _client = new Client(address, port, ClientEventDone);
            _client.OnLogEvent += (sender, objects) =>  Logger.Log(objects);
         }
         catch (Exception e)
         {
            Logger.LogError(e, "Wrong value of port - provided data cannot be parsed to number of port");
         }

         ClientEventDone.WaitOne();
         ClientEventDone.Reset();
      }
   }
}