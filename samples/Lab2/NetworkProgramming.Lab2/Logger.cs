using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NetworkProgramming.Lab2
{
   public sealed class Logger
   {
      private static Logger _instance;

      public static Logger GetInstance()
      {
         return _instance ??= new Logger();
      }

      public enum MessageType
      {
         Info, Error, Success, Client, Server
      }

      public static int LoggerBeginLine { get; set; }
      public static int ReturnLine { get; set; }
      public static ManualResetEvent CanWrite { get; set; }
      private static readonly IList<Tuple<MessageType, string>> RecoveredLogs = new List<Tuple<MessageType, string>>();
      public static event EventHandler QuitRequest;

      public static void Log(object[] args)
      {
         var type = (MessageType)args[0];

         switch (type)
         {
            case MessageType.Info:
               LogInfo((string)args[1]);
               break;
            case MessageType.Error:
               LogError((Exception)args[1], (string)args[2]);
               break;
            case MessageType.Success:
               LogSuccess((string)args[1]);
               break;
            case MessageType.Client:
               LogMsg((string)args[1]);
               break;
            case MessageType.Server:
               LogMsg((string)args[1], true);
               break;
            default:
               throw new ArgumentOutOfRangeException();
         }
      }
      public static void LogError(Exception exception, string additionalMessage)
      {
         CanWrite?.WaitOne();
         Console.SetCursorPosition(0, LoggerBeginLine);
         Console.ForegroundColor = ConsoleColor.Red;
         Console.Write(additionalMessage ?? "");
         Console.WriteLine(exception.Message + "\n" + exception.Source + "\n" + exception.StackTrace);
         Console.ForegroundColor = ConsoleColor.White;
         LoggerBeginLine = Console.CursorTop + 1;
         Console.SetCursorPosition(0, ReturnLine);
         StringBuilder recoveredLogs = new StringBuilder();
         recoveredLogs.Append(additionalMessage ?? "")
            .Append(exception.Message)
            .Append("\n")
            .Append(exception.Source)
            .Append("\n")
            .Append(exception.StackTrace)
            .Append("\n");
         RecoveredLogs.Add(Tuple.Create(MessageType.Error, recoveredLogs.ToString()));
      }

      public static void LogSuccess(string message)
      {
         CanWrite?.WaitOne();
         Console.SetCursorPosition(0, LoggerBeginLine);
         Console.ForegroundColor = ConsoleColor.Green;
         Console.Write(message ?? "");
         Console.ForegroundColor = ConsoleColor.White;
         LoggerBeginLine = Console.CursorTop + 1;
         Console.SetCursorPosition(0, ReturnLine);
         RecoveredLogs.Add(Tuple.Create(MessageType.Success, message ?? ""));
      }

      public static void LogMsg(string message, bool isServer = false)
      {
         CanWrite?.WaitOne();
         Console.SetCursorPosition(0, LoggerBeginLine);
         Console.ForegroundColor = isServer ? ConsoleColor.DarkMagenta : ConsoleColor.DarkCyan;
         Console.Write(isServer ? "FROM " : "TO ");
         Console.Write($"SERVER: {message}\n\tCOUNT:{message.ToCharArray().Length} BYTES\n");
         Console.ForegroundColor = ConsoleColor.White;
         LoggerBeginLine = Console.CursorTop + 1;
         Console.SetCursorPosition(0, ReturnLine);
         RecoveredLogs.Add(Tuple.Create(isServer ? MessageType.Server : MessageType.Client, (isServer ? "FROM " : "TO ") + $"SERVER: {message}\n\tCOUNT:{message.Length} BYTES\n"));
      }

      public static void LogInfo(string message)
      {
         CanWrite?.WaitOne();
         Console.SetCursorPosition(0, LoggerBeginLine);
         Console.ForegroundColor = ConsoleColor.Cyan;
         Console.Write($"Info: {message}");
         Console.ForegroundColor = ConsoleColor.White;
         LoggerBeginLine = Console.CursorTop + 1;
         Console.SetCursorPosition(0, ReturnLine);
         RecoveredLogs.Add(Tuple.Create(MessageType.Info, $"Info: {message}"));
         if (message.Equals("Quitting"))
         {
            QuitRequest?.Invoke(GetInstance(), EventArgs.Empty);
         }
      }

      private static void ClearLogArea()
      {
         Console.SetCursorPosition(0, ReturnLine + 1);
         Console.Write(new string(' ', Console.WindowWidth));
         Console.SetCursorPosition(0, ReturnLine + 2);
         Console.Write("Log: ");
         for (var i = ReturnLine + 3; i < LoggerBeginLine + 1; ++i)
         {
            Console.SetCursorPosition(0, i);
            Console.Write(new string(' ', Console.WindowWidth));
         }

         LoggerBeginLine = ReturnLine + 2;
      }

      public static void RecoverLogs()
      {
         ClearLogArea();
         foreach (var recoveredLog in RecoveredLogs)
         {
            Console.SetCursorPosition(0, LoggerBeginLine);
            Console.ForegroundColor = recoveredLog.Item1 switch
            {
               MessageType.Info => ConsoleColor.Cyan,
               MessageType.Error => ConsoleColor.Red,
               MessageType.Client => ConsoleColor.DarkCyan,
               MessageType.Server => ConsoleColor.DarkMagenta,
               MessageType.Success => ConsoleColor.Green,
               _ => ConsoleColor.White
            };
            Console.Write(recoveredLog.Item2);
            Console.ForegroundColor = ConsoleColor.White;
            LoggerBeginLine = Console.CursorTop + 1;
         }
      }
   }
}