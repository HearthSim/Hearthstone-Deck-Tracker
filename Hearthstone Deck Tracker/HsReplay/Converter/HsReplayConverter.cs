#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.HsReplay.Constants;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay.Converter
{
	internal class HsReplayConverter
	{
		private static readonly List<string> Converting = new List<string>();
		private static int _fallbackIndex = 1;

		public static async Task<string> Convert(List<string> log, GameStats stats, GameMetaData gameMetaData, bool includeDeck = false)
		{
			var id = gameMetaData?.GameId ?? stats?.GameId.ToString() ?? (_fallbackIndex++).ToString();
			if(Converting.Contains(id))
			{
				Log.Error($"Converting {id} already in progress.");
				return null;
			}
			Converting.Add(id);
			var output = await ConvertInternal(log, stats, gameMetaData, includeDeck);
			Converting.Remove(id);
			if(string.IsNullOrEmpty(output))
				ErrorManager.AddError("Could not convert replay", "Check the \"%AppData%/HearthstoneDeckTracker/Logs/hdt_log.txt\" for more info.");
			return output;
		}

		private static async Task<string> ConvertInternal(List<string> log, GameStats stats, GameMetaData gameMetaData, bool includeDeck)
		{
			Log.Info($"Converting hsreplay, game={{{stats}}}");
			if(!File.Exists(HsReplayExe))
			{
				Log.Warn($"{HsReplayExe} not found. Running setup.");
				var setup = await HsReplayManager.Setup();
				if(!setup)
				{
					Log.Error("Setup was not successful. Can not convert replay.");
					return null;
				}
			}
			var result = LogValidator.Validate(log);
			if(!result.Valid)
				return null;
			var tmpFile = Helper.GetValidFilePath(TmpDirPath, "tmp", "log");
			try
			{
				try
				{
					Log.Info($"Creating temp log file for converter: {tmpFile}");
					using(var sw = new StreamWriter(tmpFile))
					{
						foreach(var line in log)
							sw.WriteLine(line);
					}
				}
				catch(Exception e)
				{
					Log.Error(e);
					return null;
				}
				var converterResult = await RunExeAsync(tmpFile, stats?.StartTime, result.IsPowerTaskList);
				if(!string.IsNullOrEmpty(converterResult.Error))
				{
					Log.Error(converterResult.Error);
					return null;
				}
				if(string.IsNullOrEmpty(converterResult.Output))
				{
					Log.Error("Converter output is empty.");
					return null;
				}
				return stats != null ? XmlHelper.AddData(converterResult.Output, gameMetaData, stats, includeDeck) : converterResult.Output;
			}
			finally
			{
				try
				{
					File.Delete(tmpFile);
				}
				catch(Exception e)
				{
					Log.Error(e);
				}
			}
		}

		private static async Task<ConverterResult> RunExeAsync(string file, DateTime? time, bool usePowerTaskList)
		{
			try
			{
				return await Task.Run(() => RunExe(file, time, usePowerTaskList));
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new ConverterResult();
			}
		}

		private static ConverterResult RunExe(string file, DateTime? time, bool usePowerTaskList)
		{
			var dateString = time?.ToString("yyyy-MM-dd");
			var defaultDateArg = time.HasValue ? $"--default-date={dateString} " : "";
			var processorArg = usePowerTaskList ? "--processor=PowerTaskList " : "";
			try
			{
				var procInfo = new ProcessStartInfo
				{
					FileName = HsReplayExe,
					Arguments = defaultDateArg + processorArg + file,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					StandardOutputEncoding = Encoding.UTF8,
					UseShellExecute = false
				};
				Log.Info($"Running \"{procInfo.FileName} {procInfo.Arguments}\"");
                using(var proc = Process.Start(procInfo))
				{
					if(proc == null)
						return new ConverterResult();
					return new ConverterResult(proc.StandardOutput.ReadToEnd(), proc.StandardError.ReadToEnd());
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new ConverterResult();
			}
		}
	}

	public class ConverterResult
	{
		public string Output { get; }
		public string Error { get; }

		public ConverterResult(string output = null, string error = null)
		{
			Output = output;
			Error = error;
		}
	}
}