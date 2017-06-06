#region

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

#endregion

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class ImportingHelper
	{
		public static async Task<HtmlDocument> GetHtmlDoc(string url) => await GetHtmlDoc(url, null, null);

		public static async Task<HtmlDocument> GetHtmlDocGzip(string url)
		{
			using(var wc = new GzipWebClient())
			{
				wc.Encoding = Encoding.UTF8;
				// add an user-agent to stop some 403's
				wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0");

				var websiteContent = await wc.DownloadStringTaskAsync(new Uri(url));
				using(var reader = new StringReader(websiteContent))
				{
					var doc = new HtmlDocument();
					doc.Load(reader);
					return doc;
				}
			}
		}

		public static async Task<HtmlDocument> GetHtmlDoc(string url, string header, string headerValue)
		{
			using(var wc = new WebClient())
			{
				wc.Encoding = Encoding.UTF8;
				// add an user-agent to stop some 403's
				wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0");
				if(header != null)
					wc.Headers.Add(header, headerValue);

				var websiteContent = await wc.DownloadStringTaskAsync(new Uri(url));
				using(var reader = new StringReader(websiteContent))
				{
					var doc = new HtmlDocument();
					doc.Load(reader);
					return doc;
				}
			}
		}

		public static async Task<string> PostJson(string url, string data)
		{
			return await JsonRequest(url, data);
		}

		public static async Task<string> JsonRequest(string url, string data = null)
		{
			using(var wc = new WebClient())
			{
				wc.Encoding = Encoding.UTF8;
				wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");

				var response = "";
				if(string.IsNullOrWhiteSpace(data))
					response = await wc.DownloadStringTaskAsync(new Uri(url));
				else
					response = await wc.UploadStringTaskAsync(new Uri(url), data);

				return response;
			}
		}

		public static async Task<HtmlDocument> GetHtmlDocJs(string url)
		{
			using(var wb = new WebBrowser())
			{
				var done = false;
				var doc = new HtmlDocument();
				wb.ScriptErrorsSuppressed = true;
				//                  avoid cache
				wb.Navigate(url + "?" + DateTime.Now.Ticks);
				wb.DocumentCompleted += (sender, args) => done = true;

				while(!done)
					await Task.Delay(50);
				doc.Load(wb.DocumentStream);
				return doc;
			}
		}

		// To handle GZipped Web Content
		// http://stackoverflow.com/a/4567408/2762059
		private class GzipWebClient : WebClient
		{
			protected override WebRequest GetWebRequest(Uri address)
			{
				var request = (HttpWebRequest)base.GetWebRequest(address);
				if(request != null)
					request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				return request;
			}
		}
	}
}
