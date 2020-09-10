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
	}
}
