#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.API;
using Hearthstone_Deck_Tracker.HsReplay.Converter;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.HsReplay.Constants;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class HsReplayManager
	{
		internal static async Task ProcessPowerLog(List<string> powerLog, GameStats stats, GameMetaData metaData, bool includeDeck)
		{
			var xml = await HsReplayConverter.Convert(powerLog, stats, metaData, includeDeck);
			if(string.IsNullOrEmpty(xml))
				return;
			var rfm = new ReplayFileManager(stats);
			if(rfm.ReplayExists)
				rfm.StoreHsReplay(xml);
			var result = await HsReplayUploader.UploadXml(xml);
			if(result.Success)
			{
				stats.HsReplay = new HsReplayInfo(result.ReplayId);
				DeckStatsList.Save();
				DefaultDeckStats.Save();
			}
		}

		public static async Task<bool> ShowReplay(GameStats game)
		{
			if(game == null)
				return false;
			if(Config.Instance.ForceLocalReplayViewer)
			{
				ReplayReader.LaunchReplayViewer(game.ReplayFile);
				return true;
			}
			var rfm = new ReplayFileManager(game);
			if(!rfm.HasHsReplayFile)
				await rfm.ConvertAndStoreHsReplay();
			if(rfm.HasHsReplayFile && (!game.HsReplay?.Uploaded ?? true))
			{
				var result = await HsReplayUploader.UploadXml(rfm.HsReplay);
				if(result.Success)
				{
					game.HsReplay = new HsReplayInfo(result.ReplayId);
					if(DefaultDeckStats.Instance.DeckStats.Any(x => x.DeckId == game.DeckId))
						DefaultDeckStats.Save();
					else
						DeckStatsList.Save();
				}
			}
			if(game.HsReplay?.Uploaded ?? false)
				Helper.TryOpenUrl(game.HsReplay?.Url);
			else if(game.HasReplayFile)
				ReplayReader.LaunchReplayViewer(game.ReplayFile);
			else
				return false;
			return true;
		}

		public static async Task<bool> Setup()
		{
			try
			{
				await ApiManager.UpdateAccountStatus();
				Directory.CreateDirectory(HsReplayPath);
				Directory.CreateDirectory(TmpDirPath);
				await HsReplayUpdater.Update();
				if(!File.Exists(Msvcr100DllPath))
					File.Copy(Msvcr100DllHearthstonePath, Msvcr100DllPath);
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public static async Task ShowReplay(string fileName)
		{
			if(Config.Instance.ForceLocalReplayViewer)
			{
				ReplayReader.LaunchReplayViewer(fileName);
				return;
			}
			var rfm = new ReplayFileManager(fileName);
			if(!rfm.HasHsReplayFile)
				await rfm.ConvertAndStoreHsReplay();
			if(rfm.HasHsReplayFile)
			{
				var result = await HsReplayUploader.UploadXml(rfm.HsReplay);
				if(result.Success)
					Helper.TryOpenUrl(new HsReplayInfo(result.ReplayId).Url);
				else
					ReplayReader.LaunchReplayViewer(fileName);
			}
			else
				ReplayReader.LaunchReplayViewer(fileName);
		}
	}
}