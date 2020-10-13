using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class CardImageImporter
	{
		const string HearthstoneArtUrl = "https://art.hearthstonejson.com/v1/256x";
		private static string StoreImagesPath = Path.Combine(Config.AppDataPath, "\\Images");

		private static List<string> _succesfullyDownloadedImages = new List<string>();
		private static Dictionary<string, Task> _inProcessDownloads = new Dictionary<string, Task>();

		static CardImageImporter()
		{
			if(!Directory.Exists(StoreImagesPath))
				Directory.CreateDirectory(StoreImagesPath);
			DeleteFailedDownloads();
			_succesfullyDownloadedImages.AddRange(GetCurrentlyStoredCardids());
		}


		public static void CheckFinishedDownloads(Entity[] entities)
		{
			var toAwait = new List<Task>();
			foreach(var entity in entities)
			{
				if(CardImageIsSuccessfullyDownloaded(entity))
				{
					continue;
				}
				if(_inProcessDownloads.TryGetValue(entity.CardId, out var task))
				{
					if(task.IsCompleted)
					{
						_inProcessDownloads.Remove(entity.CardId);

						if(!task.IsFaulted && !task.IsCanceled)
							_succesfullyDownloadedImages.Add(entity.CardId);
						else
							File.Delete(StoragePathFor(entity.CardId));
						continue;
					}
					else
					{
						toAwait.Add(task);
					}
				}
				else
				{
					Log.Debug($"Requested cardimage for {entity.Name} with name {entity.CardId} without ever starting download.");
					DownloadCard(entity.CardId);
				}
			}
		}

		public static bool CardImageIsSuccessfullyDownloaded(Entity entity) => _succesfullyDownloadedImages.Contains(entity.CardId);

		public static void StartDownloadsFor(Entity[] entites) => entites.Where(x => !_succesfullyDownloadedImages.Contains(x.CardId)).ToList().ForEach(x => DownloadCard(x.CardId));

		private static bool CardDownloadStartedOrFinished(string cardId) => _succesfullyDownloadedImages.Contains(cardId) || _inProcessDownloads.ContainsKey(cardId);

		public static string StoragePathFor(string cardId) => string.Format($"{StoreImagesPath}\\{cardId}.jpg");

		public static void DownloadCard(string cardId)
		{
			if(CardDownloadStartedOrFinished(cardId))
				return;
			var requestUrl = $"{HearthstoneArtUrl}/{cardId}.jpg";
			var storageUrl = StoragePathFor(cardId);
			Log.Info($"Starting download for {cardId}");
			try
			{
				using(WebClient client = new WebClient())
				{
					_inProcessDownloads[cardId] = client.DownloadFileTaskAsync(new Uri(requestUrl), storageUrl);
					Log.Info($"Started downloading {cardId}");
				}
			}
			catch(Exception e)
			{
				Log.Error($"Unable to download {cardId}: {e.Message}");
			}
		}

		public static IEnumerable<string> GetCurrentlyStoredCardIds() => GetStoredImagesContent().Select(Path.GetFileNameWithoutExtension);

		private static void DeleteFailedDownloads()
		{
			try
			{
				var dirInfo = new DirectoryInfo(StoreImagesPath);
				dirInfo.GetFiles().Where(x => x.Length == 0).ToList().ForEach(x => File.Delete(x.FullName));
			}
			catch(Exception e)
			{
				Log.Error($"Failed to delete improperly downloaded files. Error message: {e.Message}.");
			}
		}

		static List<string> GetStoredImagesContent()
		{
			try
			{
				return Directory.GetFiles(StoreImagesPath).ToList();
			}
			catch (Exception e)
			{
				Log.Error($"Could not read the content of directory {StoreImagesPath}.");
				return null;
			}
		}

	}
}
