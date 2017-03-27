#region 

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json.Linq;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	class InstallUtils
	{
		private static InstallUtils _instance;
		public static InstallUtils Instance => _instance ?? (_instance = new InstallUtils());

		public List<Plugin> Plugins;

		private static string GetAuthor(string releaseData) => JObject.Parse(releaseData)["author"]["login"].ToString();

		private static string GetVersion(string releaseData) => JObject.Parse(releaseData)["tag_name"].ToString().Replace("v", "");

		private static string GetBody(string releaseData) => JObject.Parse(releaseData)["body"].ToString();

		private static string ReleaseUrl(string repoUrl)
		{
			try
			{
				var strarray = repoUrl.Split('/');
				strarray[2] = "api." + strarray[2];
				strarray[3] = "repos/" + strarray[3];
				return string.Join("/", strarray) + "/releases/latest";
			}
			catch(Exception)
			{
				return null;
			}
		}

		public static async Task<List<Plugin>> GetPlugins(IEnumerable<PluginWrapper> plugins)
		{
			//Create list of repo URLS to compare to.
			var repos = plugins.Select(p => ReleaseUrl(p?.Repourl)).ToList();
			Log.Info("downloading available plugin list.");
			var pluginList = new List<Plugin>();
			using(var wc = new WebClient { Headers = { ["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim" } })
			{
				wc.Headers["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim";
				var json = JObject.Parse(await wc.DownloadStringTaskAsync("https://raw.githubusercontent.com/HearthSim/HDT-Plugins/master/plugins.json"));
				pluginList.AddRange(from jToken in json["data"]
					let baseUrl = jToken["url"].ToString()
					let releaseUrl = baseUrl + "/releases/latest"
					where !repos.Contains(releaseUrl)
					select new Plugin
					{
						Author = jToken["author"].ToString(), 
						Description = jToken["description"].ToString(), 
						Name = jToken["title"].ToString(), 
						ReleaseUrl = releaseUrl, 
						Binary = jToken["binary"].ToString()
					});
				return pluginList;
			}
		}

		public static async Task<Update> GetUpdate(PluginWrapper pluginItem)
		{
			var update = new Update();
			try
			{
				var pluginBinary = Path.GetFileName(pluginItem.RelativeFilePath)?.Replace("%20", " ");
				var inTthis = Instance.Plugins.First(x => x.Binary == pluginBinary);
				if(inTthis == null)
					return update;
				pluginItem.Repourl = inTthis.ReleaseUrl;
				if(string.IsNullOrEmpty(pluginItem.Repourl))
				{
					Log.Info($"{pluginItem.Name} cannot be checked for updates. Invalid repo url {pluginItem.Repourl}.");
					update.IsUpdatable = false;
					return update;
				}
				update.IsUpdatable = true;
				update.IsUpToDate = true;
				using(var wc = new WebClient { Headers = { ["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim" } })
				{
					var release = pluginItem.Repourl;
					var releaseData = await wc.DownloadStringTaskAsync(release);
					var latestVersion = Version.Parse(GetVersion(releaseData));
					update.IsUpToDate = pluginItem.Plugin.Version >= latestVersion;
					update.Plugin = new Plugin
					{
						Author = GetAuthor(releaseData),
						Description = GetBody(releaseData),
						Name = $"{pluginItem.Name} {latestVersion}",
						ReleaseUrl = release
					};
				}
			}
			catch (Exception ex)
			{
				
				Log.Error(ex);
			}
			return update;
		}

		public static async Task<bool> UpdatePlugin(Plugin pluginItem)
		{
			try
			{
				using(var wc = new WebClient { Headers = { ["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim" } })
				{
					if(pluginItem == null) throw new Exception("Update pressed without a plugin selected.");
					var releaseUrl = pluginItem.ReleaseUrl;
					var json = JObject.Parse(await wc.DownloadStringTaskAsync(releaseUrl));
					var downloadUrl = json["assets"].First["browser_download_url"].ToString();
					var downloadFile = Path.Combine(Path.GetTempPath(), downloadUrl.Split('/').Last());
					wc.DownloadFile(downloadUrl, downloadFile);
					var stringList = new[] { downloadFile };
					return InstallPluginFile(stringList);
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}

		public static int RateLimitHit()
		{
			using(var wc = new WebClient { Headers = { ["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim" } })
			{
				var json = JObject.Parse(wc.DownloadString("https://api.github.com/rate_limit"));
				if(json["rate"]["remaining"].ToString() == "0")
				{
					return int.Parse(json["rate"]["reset"].ToString()) - int.Parse(DateTime.Now.ToUnixTime().ToString()) + 1;
				}
				return 0;
			}
		}

		public static bool InstallPluginFile(string[] files)
		{
			var dir = PluginManager.PluginDirectory.FullName;
			try
			{
				var plugins = 0;
				foreach(var pluginPath in files)
				{
					if(pluginPath.EndsWith(".dll"))
					{
						File.Copy(pluginPath, Path.Combine(dir, Path.GetFileName(pluginPath)), true);
						plugins++;
					}
					else if(pluginPath.EndsWith(".zip"))
					{
						var path = Path.Combine(dir, Path.GetFileNameWithoutExtension(pluginPath));
						if(Directory.Exists(path))
							Directory.Delete(path, true);
						ZipFile.ExtractToDirectory(pluginPath, path);
						if(Directory.GetDirectories(path).Length == 1 && Directory.GetFiles(path).Length == 0)
						{
							Directory.Delete(path, true);
							ZipFile.ExtractToDirectory(pluginPath, dir);
						}
						plugins++;
					}
				}
				return plugins > 0;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}

		public static bool UninstallPlugin(PluginWrapper plugin)
		{
			
			var parent = Directory.GetParent(plugin.RelativeFilePath);
			var pluginDirectory = PluginManager.PluginDirectory;

			try
			{
				if(parent.Name == "Plugins")
				{
					Log.Info($"Removing plugin {plugin.Plugin.Name}");
					//is our top-level plugins directory, used for single-dll plugins. Hopefully dependencies aren't directly in here.
					File.Delete(Path.Combine(pluginDirectory.FullName, plugin.RelativeFilePath.Split('/').Last()));
					return true;
				}
				else
				{
					Log.Info($"Removing plugin {plugin.Plugin.Name}");
					//Its own directory, remove dependencies too.
					Directory.Delete(Path.Combine(pluginDirectory.FullName, parent.Name), true);
					return true;
				}
			}
			catch(Exception ex)
			{

				Log.Error(ex);
				return false;
			}
		}

		public static async Task<bool> InstallRemote(Plugin plugin)
		{
			try
			{
				using(var wc = new WebClient { Headers = { ["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim" } })
				{
					var json = JObject.Parse(await wc.DownloadStringTaskAsync(plugin.ReleaseUrl));
					var downloadUrl = json["assets"].First["browser_download_url"].ToString();
					var downloadFile = Path.Combine(Path.GetTempPath(), downloadUrl.Split('/').Last());
					wc.DownloadFile(downloadUrl, downloadFile);
					var stringList = new[] { downloadFile };
					return InstallPluginFile(stringList);
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}
	}

	public class Plugin
	{
		public string Name { get; set; }
		public string Author { get; set; }
		public string Description { get; set; }
		public string ReleaseUrl { get; set; }
		public string Binary { get; set; }
	}

	public class Update
	{
		public bool IsUpdatable { get; set; }
		public bool IsUpToDate { get; set; }
		public Plugin Plugin { get; set; }
	}
}
