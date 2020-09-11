using Hearthstone_Deck_Tracker.Importing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HDTTests.ImageDownloading
{
	[TestClass]
	public class ImageDownloading
	{
		[TestMethod]
		public async Task DownloadImage()
		{
			Task download = CardImageImporter.DownloadCardAsync("BGS_004");
			await download;
		}

		[TestMethod]
		public async Task DownloadMultipleImages()
		{
			List<string> cardIds = new List<string>() { "CS2_231", "OG_300", "AT_022" };
			List<Task> toAwait = new List<Task>();
			Console.WriteLine("starting donwload at " + DateTime.Now);
			foreach(var cardId in cardIds)
			{
				Console.WriteLine("Fetching imaage with id " + cardId);
				toAwait.Add(CardImageImporter.DownloadCardAsync(cardId));
			}
			Console.WriteLine("starting wait at " + DateTime.Now);
			Console.WriteLine("finsihed loop at " + DateTime.Now);
			await Task.WhenAll(toAwait.ToArray());
			Console.WriteLine("finished all downlloads at " + DateTime.Now);
		}
	}
}
