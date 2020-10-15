using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HDTTests.ImageDownloading
{
	[TestClass]
	public class ImageDownloadingTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AssetDownloader_FailsWhenInitializedToInvalidPath()
		{
			AssetDownloader assetDownloader = new AssetDownloader("", null, null);
		}
	}
}
