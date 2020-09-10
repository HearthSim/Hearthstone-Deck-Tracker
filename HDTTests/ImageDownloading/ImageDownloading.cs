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
		public void DownloadImage()
		{
			const string imageurl = "https://art.hearthstonejson.com/v1/256x/SCH_614.jpg";
			using(WebClient client = new WebClient())
			{
				//client.DownloadFile(new Uri(imageurl), @"C:\temp\image35.png");
				// OR
				Console.Write("err");
				client.DownloadFileAsync(new Uri(imageurl), @"C:\temp\imagee35.jpg");
			}
		}
	}
}
