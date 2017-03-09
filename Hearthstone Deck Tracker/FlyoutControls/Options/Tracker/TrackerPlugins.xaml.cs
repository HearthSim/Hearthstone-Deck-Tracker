#region

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerPlugins.xaml
	/// </summary>
	public partial class TrackerPlugins : UserControl
	{
		public TrackerPlugins()
		{
			InitializeComponent();
		}

		private bool _loaded;

		private void GroupBox_Loaded(object sender, RoutedEventArgs e)
		{
			if(_loaded)
				return;
			try
			{
				ListBoxAvailable.ItemsSource = GetPlugins();
				_loaded = true;
				ListBoxUpdates.ItemsSource = GetUpdates();
			}
			catch(Exception ex)
			{
				Log.Error(ex);

			}
		}

		#region Installed


		public void Load()
		{
			ListBoxPlugins.ItemsSource = PluginManager.Instance.Plugins;
			if(ListBoxPlugins.Items.Count > 0)
				ListBoxPlugins.SelectedIndex = 0;
			else
				GroupBoxDetails.Visibility = Visibility.Hidden;
		}

		private void ListBoxPlugins_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
		}

		private void ButtonIssues_OnClick(object sender, RoutedEventArgs e)
		{
			var releases = (ListBoxPlugins.SelectedItem as PluginWrapper)?.Repourl + "/issues";
			Helper.TryOpenUrl(releases);
		}

		private void ButtonReadme_OnClick(object sender, RoutedEventArgs e)
		{
			var releases = (ListBoxPlugins.SelectedItem as PluginWrapper)?.Repourl + "#readme";
			Helper.TryOpenUrl(releases);
		}

		private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
		{
			(ListBoxPlugins.SelectedItem as PluginWrapper)?.OnButtonPress();
		}

		private void GroupBox_Drop(object sender, DragEventArgs e)
		{
			if(!e.Data.GetDataPresent(DataFormats.FileDrop))
				return;
			InstallPlugin((string[])e.Data.GetData(DataFormats.FileDrop));
		}

		private async void InstallPlugin(string[] files)
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
						ZipFile.ExtractToDirectory(pluginPath, path);
						if(Directory.GetDirectories(path).Length == 1 && Directory.GetFiles(path).Length == 0)
						{
							Directory.Delete(path, true);
							ZipFile.ExtractToDirectory(pluginPath, dir);
						}
						plugins++;
					}
				}
				if(plugins <= 0)
					return;
				var result = await Core.MainWindow.ShowMessageAsync("Plugins installed",
					$"Successfully installed {plugins} plugin(s). \n Restart now to take effect?", MessageDialogStyle.AffirmativeAndNegative);

				if(result != MessageDialogResult.Affirmative)
					return;
				Core.MainWindow.Restart();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				Core.MainWindow.ShowMessage("Error Importing Plugin", $"Please import manually to {dir}.").Forget();
			}
		}

		#endregion



		#region JSON functions

		private List<Plugin> GetPlugins()
		{
			//Create list of repo URLS to compare to.
			var repos = new List<string>();
			foreach (var p in ListBoxPlugins.Items)
			{
				var plugin = p as PluginWrapper;
				repos.Add(releaseUrl(plugin?.Repourl));
			}
			Log.Info("downloading available plugin list.");
			var pluginList = new List<Plugin>();
			var wc = new WebClient();
			wc.Headers["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim";
			var json = JObject.Parse(wc.DownloadString("https://raw.githubusercontent.com/HearthSim/HDT-Plugins/master/plugins.json"));
			foreach(var plugin in json["data"])
			{
				var baseUrl = plugin["url"].ToString();
				var releaseUrl = baseUrl + "/releases/latest";
				if(repos.Contains(releaseUrl))
					continue;
				var Plugin = new Plugin
				{
					Author = plugin["author"].ToString(),
					Description = plugin["description"].ToString(),
					Name = plugin["title"].ToString(),
					ReleaseUrl = releaseUrl
				};
				pluginList.Add(Plugin);
			}
			wc.Dispose();
			return pluginList;
		}

		private List<Plugin> GetUpdates()
		{
			try
			{
				Log.Info("Checking for updates to installed plugins.");
				var updateList = new List<Plugin>();
				if(ListBoxPlugins.Items.IsEmpty)
					return null;
				var wc = new WebClient();
				foreach(var p in ListBoxPlugins.Items)
				{
					var plugin = p as PluginWrapper;
					wc.Headers["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim";
					//have to set header each time, due to some WebClient issue of cleaing UA header after the first request.
					if(string.IsNullOrEmpty(plugin?.Repourl))
					{
						Log.Info($"{plugin.Name} cannot be checked for updates. Invalid repo url.");
						continue;
					}
					var release = releaseUrl(plugin.Repourl);
					var releaseData = wc.DownloadString(release);
					var latestVersion = Version.Parse(GetVersion(releaseData));
					if(plugin.Plugin.Version >= latestVersion)
						continue;
					var Plugin = new Plugin
					{
						Author = GetAuthor(releaseData),
						Description = GetBody(releaseData),
						Name = $"{plugin.Name} {latestVersion}",
						ReleaseUrl = release
					};
					updateList.Add(Plugin);
				}
				wc.Dispose();
				return updateList;
			}
			catch(WebException)
			{
				if(rateLimitHit())
				{
					Log.Info("User has hit github's rae limit.");
				}
				return null;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}
		private string GetAuthor(string releaseData) => JObject.Parse(releaseData)["author"]["login"].ToString();

		private string GetVersion(string releaseData) => JObject.Parse(releaseData)["tag_name"].ToString().Replace("v", "");

		private string GetBody(string releaseData) => JObject.Parse(releaseData)["body"].ToString();


		private bool rateLimitHit()
		{
			var wc = new WebClient();
			wc.Headers["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim";
			var json = JObject.Parse(wc.DownloadString("https://api.github.com/rate_limit"));
			wc.Dispose();
			if(json["rate"]["remaining"].ToString() == "0")
				return true;
			return false;
		}

		private string releaseUrl(string repoUrl)
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
		#endregion



		#region Available

		private void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var wc = new WebClient();
				wc.Headers["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim";
				var releaseUrl = (ListBoxAvailable.SelectedItem as Plugin).ReleaseUrl;
				var json = JObject.Parse(wc.DownloadString(releaseUrl));
				var downloadUrl = json["assets"].First["browser_download_url"].ToString();
				var downloadFile = Path.Combine(Path.GetTempPath(), downloadUrl.Split('/').Last());
				wc.DownloadFile(downloadUrl, downloadFile);
				var stringList = new string[] { downloadFile };
				InstallPlugin(stringList);
				wc.Dispose();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				GithubUnavailable().Forget();
			}
		}

		private async Task GithubUnavailable()
		{
			var messageText = "Unable to download plugin for an unknown reason. The error has been logged.";
			if(rateLimitHit())
			{
				messageText = "You have hit Github's rate limit.";
			}
			var result = await Core.MainWindow.ShowMessageAsync("Unable to download plugin.",
					$"{messageText}\nTry again later or manually drag-and-drop the plugin.\nGo to the manual download page now?", MessageDialogStyle.AffirmativeAndNegative);
			if(result == MessageDialogResult.Negative)
				return;
			var releaseUrl = (ListBoxAvailable.SelectedItem as Plugin).ReleaseUrl.Replace("api.", "").Replace("/repos", "");
			Helper.TryOpenUrl(releaseUrl);
		}


		#endregion

		private void ButtonUpdate_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var wc = new WebClient();
				wc.Headers["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim";
				var releaseUrl = (ListBoxUpdates.SelectedItem as Plugin).ReleaseUrl;
				var json = JObject.Parse(wc.DownloadString(releaseUrl));
				var downloadUrl = json["assets"].First["browser_download_url"].ToString();
				var downloadFile = Path.Combine(Path.GetTempPath(), downloadUrl.Split('/').Last());
				wc.DownloadFile(downloadUrl, downloadFile);
				var stringList = new string[] { downloadFile };
				InstallPlugin(stringList);
				wc.Dispose();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				GithubUnavailable().Forget();
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
}