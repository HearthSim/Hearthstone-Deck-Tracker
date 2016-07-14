#region

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.Utility.LogConfig.LogConfigConstants;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.LogConfig
{
	internal class LogConfigUpdater
	{
		private static bool _running;
		public static bool LogConfigUpdated { get; set; }
		public static bool LogConfigUpdateFailed { get; private set; }

		public static async Task Run()
		{
			if(_running)
				return;
			_running = true;
			LogConfigWatcher.Pause();
			try
			{
				if(File.Exists(LogConfigPath))
					await Helper.WaitForFileAccess(LogConfigPath, 500);
				LogConfigUpdated = CheckLogConfig();
			}
			catch
			{
				LogConfigUpdateFailed = true;
			}
			finally
			{
				LogConfigWatcher.Continue();
				_running = false;
			}
		}

		private static bool CheckLogConfig()
		{
			try
			{
				var logConfig = ReadLogConfig();
				foreach(var item in RequiredConfigItems.Where(required => logConfig.Items.All(x => x.Name != required.Name)))
					logConfig.Add(item);
				logConfig.Verify();
				if(logConfig.Updated)
					WriteLogConfig(logConfig);
				return logConfig.Updated;
			}
			catch(Exception e)
			{
				Log.Error(e);
				throw;
			}
		}

		private static void WriteLogConfig(LogConfig logConfig)
		{
			try
			{
				// ReSharper disable once ObjectCreationAsStatement
				if(File.Exists(LogConfigPath))
					new FileInfo(LogConfigPath) {IsReadOnly = false};
			}
			catch(Exception e)
			{
				Log.Error("Could not remove read-only from log.config:\n" + e);
			}
			Log.Info("Updating log.config");
			using(var sw = new StreamWriter(LogConfigPath))
				sw.Write(string.Concat(logConfig.Items));
		}

		private static LogConfig ReadLogConfig()
		{
			var logConfig = new LogConfig();
			if(!File.Exists(LogConfigPath))
				return logConfig;
			using(var sr = new StreamReader(LogConfigPath))
			{
				LogConfigItem current = null;
				string line;
				while(!sr.EndOfStream && (line = sr.ReadLine()) != null)
				{
					var match = NameRegex.Match(line);
					if(match.Success)
					{
						current = new LogConfigItem(match.Groups["value"].Value);
						logConfig.Items.Add(current);
						continue;
					}
					if(current == null)
						continue;
					if(TryParseLine(line, LogLevelRegex, ref current.LogLevel))
						continue;
					if(TryParseLine(line, FilePrintingRegex, ref current.FilePrinting))
						continue;
					if(TryParseLine(line, ConsolePrintingRegex, ref current.ConsolePrinting))
						continue;
					if(TryParseLine(line, ScreenPrintingRegex, ref current.ScreenPrinting))
						continue;
					var verbose = false;
					if(TryParseLine(line, VerboseRegex, ref verbose))
						current.Verbose = verbose;
				}
			}
			return logConfig;
		}

		private static bool TryParseLine(string line, Regex regex, ref int value)
		{
			var match = regex.Match(line);
			if(!match.Success)
				return false;
			value = int.Parse(match.Groups["value"].Value);
			return true;
		}

		private static bool TryParseLine(string line, Regex regex, ref bool value)
		{
			var match = regex.Match(line);
			if(!match.Success)
				return false;
			bool boolValue;
			if(bool.TryParse(match.Groups["value"].Value, out boolValue))
			{
				value = boolValue;
				return true;
			}
			int intValue;
			if(int.TryParse(match.Groups["value"].Value, out intValue))
			{
				value = intValue > 0;
				return true;
			}
			value = false;
			return true;
		}
	}
}