#region

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

#endregion

namespace Hearthstone_Deck_Tracker
{
	[DebuggerStepThrough]
	public static class Logger
	{
		internal static void Initialzie()
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
					using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.None))
					{
						//can access log file => no other instance of same installation running
					}
				}
				catch (Exception)
				{
					try
					{
						var errLogFile = Path.Combine(logDir, "hdt_log_err.txt");
						using (var writer = new StreamWriter(errLogFile, true))
							writer.WriteLine("[{0}]: {1}", DateTime.Now.ToLongTimeString(), "Another instance of HDT is already running.");
					}
					catch (Exception)
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
			if(logLevel <= Config.Instance.LogLevel)
				Trace.WriteLine(string.Format("[{0}] {1}: {2}", DateTime.Now.ToLongTimeString(), category, line));
		}
	}
}