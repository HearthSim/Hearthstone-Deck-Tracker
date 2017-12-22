#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public class GitHub
	{
		public static async Task<Release> CheckForUpdate(string user, string repo, Version version, bool preRelease = false)
		{
			try
			{
				Log.Info($"{user}/{repo}: Checking for updates (current={version}, pre-release={preRelease})");
				var latest = await GetLatestRelease(user, repo, preRelease);
				if(latest.Assets.Count > 0)
				{
					if(latest.GetVersion()?.CompareTo(version) > 0)
					{
						Log.Info($"{user}/{repo}: A new version is available (latest={latest.Tag}, pre-release={preRelease})");
						return latest;
					}
					Log.Info($"{user}/{repo}: We are up-to-date (latest={latest.Tag}, pre-release={preRelease})") ;
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return null;
		}

		private static async Task<Release> GetLatestRelease(string user, string repo, bool preRelease)
		{
			try
			{
				string json;
				using(var wc = new WebClient())
				{
					wc.Headers.Add(HttpRequestHeader.UserAgent, user);
					var url = $"https://api.github.com/repos/{user}/{repo}/releases";
					if(!preRelease)
						url += "/latest";
					json = await wc.DownloadStringTaskAsync(url);
				}
				return preRelease ? JsonConvert.DeserializeObject<Release[]>(json).FirstOrDefault() 
								  : JsonConvert.DeserializeObject<Release>(json);
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		public static async Task<string> DownloadRelease(Release release, string downloadDirectory)
		{
			try
			{
				var path = Path.Combine(downloadDirectory, release.Assets[0].Name);
				using(var wc = new WebClient())
					await wc.DownloadFileTaskAsync(release.Assets[0].Url, path);
				return path;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		public class Release
		{
			[JsonProperty("tag_name")]
			public string Tag { get; set; }

			[JsonProperty("assets")]
			public List<Asset> Assets { get; set; }

			public class Asset
			{
				[JsonProperty("browser_download_url")]
				public string Url { get; set; }

				[JsonProperty("name")]
				public string Name { get; set; }
			}

			public Version GetVersion() => Version.TryParse(Tag.Replace("v", ""), out Version v) ? v : null;
		}
	}
}
