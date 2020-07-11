using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NetworkProgramming.Lab2
{
	public sealed class Logger
	{
		private static Logger _instance;

		private static Logger GetInstance()
		{
			return _instance ??= new Logger();
		}

		public enum MessageType
		{
			Info,
			Error,
			Success,
			Client,
			Server
		}

		public static int LoggerBeginLine { get; set; }
		public static int ReturnLine { get; set; }
		public static AutoResetEvent CanWrite { get; set; }

		private static readonly IList<Tuple<MessageType, string>>
			RecoveredLogs = new List<Tuple<MessageType, string>>();

		public static event EventHandler QuitRequest;

		public static void Log(object[] args)
		{
			var type = (MessageType) args[0];

			switch (type)
			{
				case MessageType.Info:
					LogInfo((string) args[1]);
					break;
				case MessageType.Error:
					LogError((Exception) args[1], (string) args[2]);
					break;
				case MessageType.Success:
					LogSuccess((string) args[1]);
					break;
				case MessageType.Client:
					LogMsg((string) args[1]);
					break;
				case MessageType.Server:
					LogMsg((string) args[1], true);
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
			var message = (additionalMessage ?? "") + exception.Message + "\n" +
						  (exception.InnerException is { } e ? $"{e.Message}\n" : "");
			Console.Write(message);
			Console.ForegroundColor = ConsoleColor.White;
			LoggerBeginLine = Console.CursorTop + 1;
			Console.SetCursorPosition(0, ReturnLine);
			RecoveredLogs.Add(Tuple.Create(MessageType.Error, message));
			CanWrite?.Set();
		}

		private static void LogSuccess(string message)
		{
			CanWrite?.WaitOne();
			Console.SetCursorPosition(0, LoggerBeginLine);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write(message ?? "");
			var overlap = Overlap(message ?? "");
			Console.ForegroundColor = ConsoleColor.White;
			LoggerBeginLine = LoggerBeginLine + overlap + 1;
			Console.SetCursorPosition(0, ReturnLine);
			RecoveredLogs.Add(Tuple.Create(MessageType.Success, message ?? ""));
			CanWrite?.Set();
		}

		private static void LogMsg(string message, bool isServer = false)
		{
			CanWrite?.WaitOne();
			Console.SetCursorPosition(0, LoggerBeginLine);
			Console.ForegroundColor = isServer ? ConsoleColor.DarkMagenta : ConsoleColor.DarkCyan;
			message = (isServer ? "FROM " : "TO ") + $"SERVER: {message}\n\tCOUNT:{message.Length} BYTES\n";
			var overlap = Overlap(message);
			Console.Write(message);
			Console.ForegroundColor = ConsoleColor.White;
			LoggerBeginLine = LoggerBeginLine + overlap + 1;
			Console.SetCursorPosition(0, ReturnLine);
			RecoveredLogs.Add(Tuple.Create(isServer ? MessageType.Server : MessageType.Client, message));
			CanWrite?.Set();
		}

		public static void LogInfo(string message)
		{
			CanWrite?.WaitOne();
			Console.SetCursorPosition(0, LoggerBeginLine);
			Console.ForegroundColor = ConsoleColor.Cyan;
			var baseMessage = message;
			message = $"Info: {message}";
			var overlap = Overlap(message);
			Console.Write(message);
			Console.ForegroundColor = ConsoleColor.White;
			LoggerBeginLine = LoggerBeginLine + 1 + overlap;
			Console.SetCursorPosition(0, ReturnLine);
			RecoveredLogs.Add(Tuple.Create(MessageType.Info, message));
			if (baseMessage.Equals("Quitting"))
			{
				QuitRequest?.Invoke(GetInstance(), EventArgs.Empty);
			}

			CanWrite?.Set();
		}

		private static int Overlap(string message)
		{
			var overlap = message.Sum(c => c == '\n' ? 1 : 0);
			overlap += (int) (Math.Ceiling(overlap / (double) Console.BufferWidth));
			return overlap;
		}

		private static void ClearLogArea()
		{
			CanWrite?.Reset();
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
			foreach (var (item1, item2) in RecoveredLogs)
			{
				Console.SetCursorPosition(0, LoggerBeginLine);
				Console.ForegroundColor = item1 switch
										  {
											  MessageType.Info => ConsoleColor.Cyan,
											  MessageType.Error => ConsoleColor.Red,
											  MessageType.Client => ConsoleColor.DarkCyan,
											  MessageType.Server => ConsoleColor.DarkMagenta,
											  MessageType.Success => ConsoleColor.Green,
											  _ => ConsoleColor.White
										  };
				Console.Write(item2);
				Console.ForegroundColor = ConsoleColor.White;
				LoggerBeginLine = Console.CursorTop + 1;
			}
		}
	}
}