#region

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Plugins
{
	/// <summary>
	/// Interaction logic for TrackerPlugins.xaml
	/// </summary>
	public partial class OptionsPluginsAvailable
	{
		public OptionsPluginsAvailable()
		{
			InitializeComponent();
			var plugins = InstallUtils.GetPlugins(PluginManager.Instance.Plugins);
			InstallUtils.Instance.Plugins = plugins;
		}

		private bool _loaded;

		private void GroupBox_Loaded(object sender, RoutedEventArgs e)
		{
			var dir = PluginManager.PluginDirectory;
			if(_loaded)
				return;
			try
			{
				_loaded = true;
				var plugins = InstallUtils.Instance.Plugins;
				var availablePlugins = plugins.Where(p => Directory.GetFiles(dir.FullName, p.Binary, SearchOption.AllDirectories).FirstOrDefault() == null).ToList();
				ListBoxAvailable.ItemsSource = availablePlugins;
				
				_loaded = true;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
		{
			//Localization: should use key SplashScreen_Text_Installing
			ButtonDownload.Content = "INSTALLING...";
			ButtonDownload.IsEnabled = false;

			//Make cursor look busy
			Mouse.OverrideCursor = Cursors.Wait;

			var plugin = ListBoxAvailable.SelectedItem as Plugin;
			if(plugin == null)
				return;
			if(InstallUtils.InstallRemote(plugin))
			{
				var newPlugins = PluginManager.Instance.SyncPlugins();
				PluginManager.Instance.LoadPlugins(newPlugins);
				Core.MainWindow.ShowMessage($"Successfully installed {plugin.Name}", "").Forget();
				ButtonDownload.IsEnabled = true;
				ButtonDownload.Content = "INSTALL";
				Mouse.OverrideCursor = null;
			}
			else
			{
				GithubUnavailable($"Unable to install {plugin.Name}.", plugin).Forget();
				ButtonDownload.IsEnabled = true;
				ButtonDownload.Content = "INSTALL";
				Mouse.OverrideCursor = null;
			}
		}

		private async Task GithubUnavailable(string messageText, Plugin plugin)
		{
			if(InstallUtils.RateLimitHit() > 0)
			{
				messageText = "You have hit GitHub's rate limit.";
			}
			var result = await Core.MainWindow.ShowMessageAsync("Unable to download plugin.",
					$"{messageText}\nTry again in {InstallUtils.RateLimitHit()} or manually drag-and-drop the plugin.\nGo to the manual download page now?", MessageDialogStyle.AffirmativeAndNegative);
			if(result == MessageDialogResult.Negative || plugin == null)
				return;
			var releaseUrl = plugin.ReleaseUrl.Replace("api.", "").Replace("/repos", "");
			Helper.TryOpenUrl(releaseUrl);
		}

		private void ButtonDetails_OnClick(object sender, RoutedEventArgs e)
		{
			var url =
				((Plugin) ListBoxAvailable.SelectedItem).ReleaseUrl.Replace("api.", "").Replace("repos/", "").Replace(
					"/releases/latest", "") + "#readme";
			Helper.TryOpenUrl(url);
		}
	}
}