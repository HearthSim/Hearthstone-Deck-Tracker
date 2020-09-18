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
		const string imageurl = "https://art.hearthstonejson.com/v1/256x/SCH_614.jpg";
		const string HearthstoneArtUrl = "https://art.hearthstonejson.com/v1/256x";
		private static string StoreImagesPath = Config.AppDataPath + "\\Images";

		private static List<string> _succesfullyDownloadedImages = new List<string>();
		private static Dictionary<string, Task> _inProcessDownloads = new Dictionary<string, Task>();

		/*Proposed architecture:
		 * Have two collections: one is a list of strings that have completed a successful download.
		 * The other is a dictionary with key of the cardid being downloaded, and the value being the task associated with that download. 
		 * Once a download is complete, the cardid is removed from the dictioanry and added to the list
		 * 
		 * As a note: on initialization we'll also check for all the ones currently stored and add it to the list
		 * 
		 * User plays opponent. This generates a list of cardids for which we will need images from which to eventually build  aboard history.
		 * We take this set of cardids. those that are in neither of the above collections (ie aren't downloaded or started) have their downloads started, a process which adds them to the dictionary
		 * 
		 * 
		 * When a user mouses over past board, we have a set of cardids to look for. for all that are in the dictionary we check to see if the task is complete. if yes, move to list. if no, await on task.
		 * missing from both lists, which shouldn't be possible but if it is we'll raise an error and start the download.
		 * 
		 * once they're all done we'll render the boadr as is done in detectmouseoverlay stuff
		 */


		static CardImageImporter()
		{
			CheckForAndAddImageFolder();
			_succesfullyDownloadedImages.AddRange(GetCurrentlyStoredCardids());
		}

		public static async Task FinishDownloadingEntites(Entity[] entities)
		{
			var toAwait = new List<Task>();
			Console.WriteLine("beggining wait for finish");
			foreach(var entity in entities)
			{
				if(CardImageIsDownloaded(entity))
				{
					Console.WriteLine(entity.Name + " was already downloaded");
					continue;
				}
				if(_inProcessDownloads.TryGetValue(entity.CardId, out var task))
				{
					Console.WriteLine(entity.Name + " was found in inprocess download");
					if(task.IsCompleted)
					{
						Console.WriteLine("it's task was completeed");
						_inProcessDownloads.Remove(entity.CardId);
						_succesfullyDownloadedImages.Add(entity.CardId);
						continue;
					}
					else
					{
						toAwait.Add(task);
						Console.WriteLine("was not finished, adding to towait");
					}
				}
				else
				{
					Log.Debug($"Requested cardimage for {entity.Name} with name {entity.CardId} without ever starting download.");
					DownloadCard(entity.CardId);
				}
			}
			Console.WriteLine("starting wait all " + DateTime.Now + " with " + toAwait.Count + " waiting for");
			await Task.WhenAll(toAwait);
			Console.WriteLine("finsihed wait all " + DateTime.Now);

		}

		private static bool CardImageIsDownloaded(Entity entity) => _succesfullyDownloadedImages.Contains(entity.CardId);

		public static void StartDownloadsFor(Entity[] entites) => entites.Where(x => !_succesfullyDownloadedImages.Contains(x.CardId)).ToList().ForEach(x => DownloadCard(x.CardId));

		private static bool CardDownloadStartedOrFinished(string cardId) => _succesfullyDownloadedImages.Contains(cardId) || _inProcessDownloads.ContainsKey(cardId);

		public static string StoragePathFor(string cardId) => string.Format($"{StoreImagesPath}\\{cardId}.jpg");

		public static void DownloadCard(string cardId)
		{
			if(CardDownloadStartedOrFinished(cardId))
				return;
			var requestUrl = string.Format($"{HearthstoneArtUrl}/{cardId}.jpg");
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
				Console.WriteLine(e.Message + " was the error :(");
			}
		}

		public static List<string> GetCurrentlyStoredCardids()
		{
			var toReturn = GetStoredImagesContent().Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
			foreach(var name in toReturn)
			{
				Console.WriteLine(name);
			}
			return toReturn;
		}

		static List<string> GetStoredImagesContent()
		{
			try
			{
				return Directory.GetFiles(StoreImagesPath).ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Could not read the content of directory {StoreImagesPath}.");
				return null;
			}
		}

		private static List<FileInfo> GetImageInfos() => new DirectoryInfo(StoreImagesPath).GetFiles().ToList();

		private static bool ImageFolderExists() => Directory.Exists(StoreImagesPath);

		private static void CheckForAndAddImageFolder()
		{
			if(!ImageFolderExists())
				Directory.CreateDirectory(StoreImagesPath);
		}

	}
}
