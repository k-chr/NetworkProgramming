using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NetworkProgramming.Lab1
{
   public sealed class Logger
   {

      enum MessageType
      {
         INFO,ERROR,SUCCESS,CLIENT,SERVER
      }

      public static int LoggerBeginLine { get; set; }
      public static int ReturnLine { get; set; }
      public static ManualResetEvent CanWrite { get; set; }
      private static IList<Tuple<MessageType, string>> _recoveredLogs = new List<Tuple<MessageType, string>>();
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
         _recoveredLogs.Add(Tuple.Create(MessageType.ERROR, recoveredLogs.ToString()));
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
         _recoveredLogs.Add(Tuple.Create(MessageType.SUCCESS, message ?? ""));
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
         _recoveredLogs.Add(Tuple.Create(isServer ? MessageType.SERVER: MessageType.CLIENT, (isServer ? "FROM " : "TO ") + $"SERVER: {message}\n\tCOUNT:{message.Length} BYTES\n"));
      }

      public static void LogInfo(string message)
      {
         CanWrite?.WaitOne();
         Console.SetCursorPosition(0, LoggerBeginLine);
         Console.ForegroundColor = ConsoleColor.Cyan;
         Console.Write($"INFO: {message}");
         Console.ForegroundColor = ConsoleColor.White;
         LoggerBeginLine = Console.CursorTop + 1;
         Console.SetCursorPosition(0, ReturnLine);
         _recoveredLogs.Add(Tuple.Create(MessageType.INFO, $"INFO: {message}"));
      }

      private static void ClearLogArea()
      {
         Console.SetCursorPosition(0, ReturnLine + 1);
         Console.Write(new string(' ', Console.WindowWidth));
         Console.SetCursorPosition(0,ReturnLine+2);
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
         foreach (var recoveredLog in _recoveredLogs)
         {
            Console.SetCursorPosition(0, LoggerBeginLine);
            Console.ForegroundColor = recoveredLog.Item1 switch
            {
               MessageType.INFO => ConsoleColor.Cyan,
               MessageType.ERROR => ConsoleColor.Red,
               MessageType.CLIENT => ConsoleColor.DarkCyan,
               MessageType.SERVER => ConsoleColor.DarkMagenta,
               MessageType.SUCCESS => ConsoleColor.Green,
               _ => ConsoleColor.White
            };
            Console.Write(recoveredLog.Item2);
            Console.ForegroundColor = ConsoleColor.White;
            LoggerBeginLine = Console.CursorTop + 1;
         }
      }
   }
}