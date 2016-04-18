#region

using System;
using System.IO;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.API;
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
	}
}