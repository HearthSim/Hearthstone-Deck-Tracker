#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

#endregion

namespace Hearthstone_Deck_Tracker
{
	[DebuggerStepThrough]
	public static class Logger
	{
		private const int MaxLogFileAge = 2;
		private const int KeepOldLogs = 25;

		internal static void Initialize()
		{
			Trace.AutoFlush = true;
			var logDir = Path.Combine(Config.Instance.DataDir, "Logs");
			var logFile = Path.Combine(logDir, "hdt_log.txt");
			if(!Directory.Exists(logDir))
				Directory.CreateDirectory(logDir);
			else
			{
				try
				{
					var fileInfo = new FileInfo(logFile);
					if(fileInfo.Exists)
					{
						using(var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.None))
						{
							//can access log file => no other instance of same installation running
						}
						File.Move(logFile, logFile.Replace(".txt", "_" + DateTime.Now.ToUnixTime() + ".txt"));
						//keep logs from the last 2 days plus 25 before that
						foreach(var file in
							new DirectoryInfo(logDir).GetFiles("hdt_log*")
							                         .Where(x => x.LastWriteTime < DateTime.Now.AddDays(-MaxLogFileAge))
							                         .OrderByDescending(x => x.LastWriteTime)
							                         .Skip(KeepOldLogs))
						{
							try
							{
								File.Delete(file.FullName);
							}
							catch
							{
							}
						}
					}
					else
						File.Create(logFile).Dispose();
				}
				catch(Exception)
				{
					try
					{
						var errLogFile = Path.Combine(logDir, "hdt_log_err.txt");
						using(var writer = new StreamWriter(errLogFile, true))
							writer.WriteLine("[{0}]: {1}", DateTime.Now.ToLongTimeString(), "Another instance of HDT is already running.");
						MessageBox.Show("Another instance of Hearthstone Deck Tracker is already running.", "Error starting Hearthstone Deck Tracker",
						                MessageBoxButton.OK, MessageBoxImage.Error);
					}
					catch(Exception)
					{
					}
					Application.Current.Shutdown();
					return;
				}
			}
			Trace.Listeners.Add(new TextWriterTraceListener(new StreamWriter(logFile, false)));
		}

		/// <summary>
		/// Writes line to trace
		/// </summary>
		public static void WriteLine(string line, int logLevel = 0)
		{
			WriteLine(line, "", logLevel);
		}

		/// <summary>
		/// Writes line to trace
		/// </summary>		
		public static void WriteLine(string line, string category, int logLevel = 0)
		{
#if (!DEBUG)
			if(logLevel <= Config.Instance.LogLevel)
#endif
				Trace.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {category}: {line}");
		}
	}
}