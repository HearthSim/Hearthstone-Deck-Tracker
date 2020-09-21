using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility.Logging;
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
		public void ReadDirectoryContents()
		{
			CardImageImporter.GetCurrentlyStoredCardids();
		}
	}
}
