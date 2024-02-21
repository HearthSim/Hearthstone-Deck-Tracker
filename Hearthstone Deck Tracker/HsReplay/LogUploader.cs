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

		public static async Task<bool> Upload(string[] logLines, GameMetaData? gameMetaData, GameStats? game)
		{
			var uploadId = game?.GameId.GetHashCode() ?? string.Join("", logLines.Take(100)).GetHashCode();
			var item = new UploaderItem(uploadId);
			if(InProgress.Contains(item))
			{
				Log.Info($"{item.Id} already in progress. Waiting for it to complete...");
				InProgress.Add(item);
				return await item.Success;
			}
			InProgress.Add(item);
			Log.Info($"Uploading {item.Id}...");
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
			Log.Info($"{item.Id} complete. Success={success}");
			foreach(var waiting in InProgress.Where(x => x.Id == item.Id))
				waiting.Complete(success);
			InProgress.RemoveAll(x => x.Id == item.Id);
			return success;
		}

		private static async Task<bool> TryUpload(string[] logLines, GameMetaData? gameMetaData, GameStats? game, bool submitFailure)
		{
			try
			{
				game?.HsReplay.UploadTry();
				var lines = logLines.SkipWhile(x => !x.Contains("CREATE_GAME")).ToArray();
				var metaData = UploadMetaDataGenerator.Generate(gameMetaData, game);
				Log.Info("Creating upload request...");
				var uploadRequest = await ApiWrapper.CreateUploadRequest(metaData);
				Log.Info("Upload Id: " + uploadRequest.ShortId);
				await ApiWrapper.UploadLog(uploadRequest, lines);
				Log.Info("Upload complete");
				if(game != null)
				{
					game.HsReplay.UploadId = uploadRequest.ShortId;
					game.HsReplay.ReplayUrl = uploadRequest.ReplayUrl;
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

			public int Id { get; }

			public UploaderItem(int id)
			{
				Id = id;
			}

			public void Complete(bool result) => _tcs.SetResult(result);

			public Task<bool> Success => _tcs.Task;
		}
	}
}
