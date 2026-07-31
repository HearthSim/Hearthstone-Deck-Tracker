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
			try
			{
				LogConfigUpdated = await Run(LogConfigPath);
			}
			catch
			{
				LogConfigUpdateFailed = true;
			}
		}

		public static async Task<bool> Run(string path)
		{
			if(_running)
				return false;
			_running = true;
			LogConfigWatcher.Pause();
			try
			{
				if(File.Exists(path))
					await Helper.WaitForFileAccess(path, 500);
				return CheckLogConfig(path);
			}
			finally
			{
				LogConfigWatcher.Continue();
				_running = false;
			}
		}

		internal static bool CheckLogConfig(string path)
		{
			try
			{
				var logConfig = ReadLogConfig(path);
				foreach(var item in RequiredConfigItems.Where(required => logConfig.Items.All(x => x.Name != required.Name)))
					logConfig.Add(item);
				logConfig.Verify();
				if(logConfig.Updated)
					WriteLogConfig(logConfig, path);
				return logConfig.Updated;
			}
			catch(Exception e)
			{
				Log.Error(e);
				throw;
			}
		}

		private static void WriteLogConfig(LogConfig logConfig, string path)
		{
			if(File.Exists(path))
			{
				try
				{
					// ReSharper disable once ObjectCreationAsStatement
					new FileInfo(path) { IsReadOnly = false };
				}
				catch(Exception e)
				{
					Log.Error("Could not remove read-only from log.config:\n" + e);
				}
			}
			else
			{
				var dir = Path.GetDirectoryName(path);
				if(dir != null && !Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
					Log.Info($"Created directory {dir}");
				}
			}

			Log.Info($"Updating {path}");
			using(var sw = new StreamWriter(path))
				sw.Write(string.Concat(logConfig.Items));
		}

		private static LogConfig ReadLogConfig(string path)
		{
			var logConfig = new LogConfig();
			if(!File.Exists(path))
				return logConfig;
			using(var sr = new StreamReader(path))
			{
				LogConfigItem? current = null;
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
			if(bool.TryParse(match.Groups["value"].Value, out var boolValue))
			{
				value = boolValue;
				return true;
			}
			if(int.TryParse(match.Groups["value"].Value, out var intValue))
			{
				value = intValue > 0;
				return true;
			}
			value = false;
			return true;
		}
	}
}
