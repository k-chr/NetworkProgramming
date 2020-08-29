using System;
using System.IO;
using System.Threading;
using CustomControls.Models;

namespace TimeProjectServices.Services
{
	public class LogDumper
	{
		private readonly string _path;
		private static readonly ReaderWriterLock ReaderWriterLock = new ReaderWriterLock();
		public LogDumper(string path) : this(ref path) => _path = path;

		private LogDumper(ref string path) =>
			File.WriteAllTextAsync(path, $"[Info] [{DateTime.Now}] Started logging session\n");

		public async void DumpLog(InternalMessageModel log)
		{
			try
			{
				ReaderWriterLock.AcquireWriterLock(1000);
				await File.AppendAllTextAsync(_path, log.ToString());
			}
			catch (Exception)
			{
				//ignored
			}
			finally
			{
				try
				{
					ReaderWriterLock.ReleaseWriterLock();
				}
				catch (Exception e)
				{
					//ignored
					Console.WriteLine(e);
				}
			}
		}

		public async void End() =>
			await File.AppendAllTextAsync(_path, $"[Info] [{DateTime.Now}] Disposed logging session");
	}
}