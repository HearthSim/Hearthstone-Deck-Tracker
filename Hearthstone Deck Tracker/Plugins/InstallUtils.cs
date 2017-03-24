#region 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json.Linq;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	static class InstallUtils
	{

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

		public static List<Plugin> GetPlugins(ObservableCollection<PluginWrapper> plugins)
		{
			//Create list of repo URLS to compare to.
			var repos = plugins.Select(p => ReleaseUrl(p?.Repourl)).ToList();
			Log.Info("downloading available plugin list.");
			var pluginList = new List<Plugin>();
			using(var wc = new WebClient { Headers = { ["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim" } })
			{
				wc.Headers["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim";
				var json = JObject.Parse(wc.DownloadString("https://raw.githubusercontent.com/HearthSim/HDT-Plugins/master/plugins.json"));
				foreach(var jToken in json["data"])
				{
					var baseUrl = jToken["url"].ToString();
					var releaseUrl = baseUrl + "/releases/latest";
					if(repos.Contains(releaseUrl))
						continue;
					var plugin = new Plugin
					{
						Author = jToken["author"].ToString(),
						Description = jToken["description"].ToString(),
						Name = jToken["title"].ToString(),
						ReleaseUrl = releaseUrl
					};
					pluginList.Add(plugin);
				}
				return pluginList;
			}
		}

		public static Update GetUpdate(PluginWrapper pluginItem)
		{
			var update = new Update();
			if(string.IsNullOrEmpty(pluginItem?.Repourl))
			{
				if(pluginItem != null) Log.Info($"{pluginItem.Name} cannot be checked for updates. Invalid repo url.");
				update.IsUpdatable = false;
				return update;
			}
			update.IsUpdatable = true;
			using (var wc = new WebClient { Headers = { ["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim" } })
			{
				var release = ReleaseUrl(pluginItem.Repourl);
				var releaseData = wc.DownloadString(release);
				var latestVersion = Version.Parse(GetVersion(releaseData));
				if(pluginItem.Plugin.Version >= latestVersion)
					update.IsUpToDate = true;
				update.Plugin = new Plugin
				{
					Author = GetAuthor(releaseData),
					Description = GetBody(releaseData),
					Name = $"{pluginItem.Name} {latestVersion}",
					ReleaseUrl = release
				};
			}
			return update;
		}

		public static bool UpdatePlugin(Plugin pluginItem)
		{
			try
			{
				using(var wc = new WebClient { Headers = { ["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim" } })
				{
					if(pluginItem == null) throw new Exception("Update pressed without a plugin selected.");
					var releaseUrl = pluginItem.ReleaseUrl;
					var json = JObject.Parse(wc.DownloadString(releaseUrl));
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

				//var result = await Core.MainWindow.ShowMessageAsync("Plugins installed",
				//	$"Successfully installed {plugins} plugin(s). \n Restart now to take effect?", MessageDialogStyle.AffirmativeAndNegative);
				//
				//if(result != MessageDialogResult.Affirmative)
				//	return;
				//Core.MainWindow.Restart();
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

		public static bool InstallRemote(Plugin plugin)
		{
			try
			{
				using(var wc = new WebClient { Headers = { ["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim" } })
				{
					var releaseUrl = plugin.ReleaseUrl;
					var json = JObject.Parse(wc.DownloadString(releaseUrl));
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
	}

	public class Update
	{
		public bool IsUpdatable { get; set; }
		public bool IsUpToDate { get; set; }
		public Plugin Plugin { get; set; }
	}
}
