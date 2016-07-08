#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Logging
{
	[DebuggerStepThrough]
	public class Log
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
					MessageBox.Show("Another instance of Hearthstone Deck Tracker is already running.", 
						"Error starting Hearthstone Deck Tracker", MessageBoxButton.OK, MessageBoxImage.Error);
					Application.Current.Shutdown();
					return;
				}
			}
			try
			{
				Trace.Listeners.Add(new TextWriterTraceListener(new StreamWriter(logFile, false)));	
			}
			catch (Exception ex)
			{
				ErrorManager.AddError("Can not access log file.", ex.ToString());
			}
		}

		public static void WriteLine(string msg, LogType type, [CallerMemberName] string memberName = "",
									 [CallerFilePath] string sourceFilePath = "")
		{
#if (!DEBUG)
			if(type == LogType.Debug && Config.Instance.LogLevel == 0)
				return;
#endif
			var file = sourceFilePath?.Split('/', '\\').LastOrDefault()?.Split('.').FirstOrDefault();
			Trace.WriteLine($"{DateTime.Now.ToLongTimeString()}|{type}|{file}.{memberName} >> {msg}");
		}

		public static void Debug(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Debug, memberName, sourceFilePath);

		public static void Info(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Info, memberName, sourceFilePath);

		public static void Warn(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Warning, memberName, sourceFilePath);

		public static void Error(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Error, memberName, sourceFilePath);

		public static void Error(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(ex.ToString(), LogType.Error, memberName, sourceFilePath);
	}
}