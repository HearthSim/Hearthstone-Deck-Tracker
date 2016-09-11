#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.Utility;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Toasts;
using Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls;
using HSReplay.LogValidation;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class ReplayLauncher
	{
		public static async Task<bool> ShowReplay(GameStats game, bool showToast)
		{
			if(game == null)
				return false;
			if(Config.Instance.ForceLocalReplayViewer)
			{
				ReplayReader.LaunchReplayViewer(game.ReplayFile, false);
				return true;
			}
			Action<ReplayProgress> setToastStatus = null;
			if(game.HasReplayFile && !game.HsReplay.Uploaded)
			{
				if(showToast)
					setToastStatus = ToastManager.ShowReplayProgressToast();
				var log = GetLogFromHdtReplay(game.ReplayFile).ToArray();
				var validationResult = LogValidator.Validate(log);
				if(validationResult.IsValid)
					await LogUploader.Upload(log, null, game);
				else
				{
					Log.Error("Invalid log: " + validationResult.Reason);
					game.HsReplay.Unsupported = true;
				}
				if(DefaultDeckStats.Instance.DeckStats.Any(x => x.DeckId == game.DeckId))
					DefaultDeckStats.Save();
				else
					DeckStatsList.Save();
			}
			if(game.HsReplay?.Uploaded ?? false)
			{
				setToastStatus?.Invoke(ReplayProgress.Complete);
				Helper.TryOpenUrl(game.HsReplay?.Url);
			}
			else if(game.HasReplayFile)
			{
				setToastStatus?.Invoke(ReplayProgress.Error);
				ReplayReader.LaunchReplayViewer(game.ReplayFile, true);
			}
			else
			{
				setToastStatus?.Invoke(ReplayProgress.Error);
				return false;
			}
			return true;
		}

		private static List<string> GetLogFromHdtReplay(string file)
		{
			var path = Path.Combine(Config.Instance.ReplayDir, file);
			if(!File.Exists(path))
				return new List<string>();
			try
			{
				using(var fs = new FileStream(path, FileMode.Open))
				using(var archive = new ZipArchive(fs, ZipArchiveMode.Read))
				using(var sr = new StreamReader(archive.GetEntry("output_log.txt").Open()))
					return sr.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new List<string>();
			}
		}

		public static async Task ShowReplay(string fileName, bool showToast)
		{
			if(Config.Instance.ForceLocalReplayViewer)
			{
				ReplayReader.LaunchReplayViewer(fileName, false);
				return;
			}
			Action<ReplayProgress> setToastStatus = null;
			var log = GetLogFromHdtReplay(fileName).ToArray();
			var validationResult = LogValidator.Validate(log);
			if(validationResult.IsValid)
			{
				if(showToast)
					setToastStatus = ToastManager.ShowReplayProgressToast();
				setToastStatus?.Invoke(ReplayProgress.Uploading);
				var file = new FileInfo(fileName);
				var hsBuild = BuildDates.GetByDate(file.LastWriteTime);
				var metaData = hsBuild != null ? new GameMetaData() {HearthstoneBuild = hsBuild} : null;
				var gameStats = hsBuild != null ? new GameStats() {StartTime = file.LastWriteTime} : null;
				var success = await LogUploader.Upload(log.ToArray(), metaData, gameStats);
				if(success)
					Helper.TryOpenUrl(gameStats?.HsReplay?.Url);
				else
					ReplayReader.LaunchReplayViewer(fileName, true);
			}
			else
			{
				Log.Error("Invalid log: " + validationResult.Reason);
				ReplayReader.LaunchReplayViewer(fileName, true);
			}
			setToastStatus?.Invoke(ReplayProgress.Complete);
		}
	}
}
