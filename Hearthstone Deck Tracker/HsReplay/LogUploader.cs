#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.Utility;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class LogUploader
	{
		private static readonly List<UploaderItem> InProgress = new List<UploaderItem>();

		public static async Task<bool> Upload(string[] logLines, GameMetaData gameMetaData, GameStats game)
		{
			var log = string.Join(Environment.NewLine, logLines);
			var item = new UploaderItem(log.GetHashCode());
			if(InProgress.Contains(item))
			{
				Log.Info($"{item.Hash} already in progress. Waiting for it to complete...");
				InProgress.Add(item);
				return await item.Success;
			}
			InProgress.Add(item);
			Log.Info($"Uploading {item.Hash}...");
			var success = false;
			try
			{
				success = await TryUpload(logLines, gameMetaData, game, true);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				Influx.OnGameUploadFailed();
			}
			Log.Info($"{item.Hash} complete. Success={success}");
			foreach(var waiting in InProgress.Where(x => x.Hash == item.Hash))
				waiting.Complete(success);
			InProgress.RemoveAll(x => x.Hash == item.Hash);
			return success;
		}

		private static async Task<bool> TryUpload(string[] logLines, GameMetaData gameMetaData, GameStats game, bool submitFailure)
		{
			try
			{
				game?.HsReplay.UploadTry();
				Influx.OnGameUpload(game?.HsReplay.UploadTries ?? 1);
				//var tsParser = new TimeStampParser(game?.StartTime ?? DateTime.MinValue);
				//logLines = logLines.Select(tsParser.Parse).ToArray();
				var metaData = UploadMetaDataGenerator.Generate(logLines, gameMetaData, game);
				var uploadRequest = await ApiWrapper.CreateUploadRequest(metaData);
				await ApiWrapper.UploadLog(uploadRequest, logLines);
				if(game != null)
				{
					game.HsReplay.UploadId = uploadRequest.ShortId;
					if(DefaultDeckStats.Instance.DeckStats.Any(x => x.DeckId == game.DeckId))
						DefaultDeckStats.Save();
					else
						DeckStatsList.Save();
				}
				return true;
			}
			catch(WebException ex)
			{
				Log.Error(ex);
				if(submitFailure)
					Influx.OnGameUploadFailed(ex.Status);
				return false;
			}
		}

		public static async Task<bool> FromFile(string filePath)
		{
			string content;
			using(var sr = new StreamReader(filePath))
				content = sr.ReadToEnd();
			return await Upload(content.Split(new []{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToArray(), null, null);
		}

		public class UploaderItem
		{
			private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

			public int Hash { get; }

			public UploaderItem(int hash)
			{
				Hash = hash;
			}

			public override bool Equals(object obj)
			{
				var uObj = obj as UploaderItem;
				return uObj != null && Equals(uObj);
			}

			public override int GetHashCode() => Hash;

			public bool Equals(UploaderItem obj) => obj.Hash == Hash;

			public void Complete(bool result) => _tcs.SetResult(result);

			public Task<bool> Success => _tcs.Task;
		}
	}
}
