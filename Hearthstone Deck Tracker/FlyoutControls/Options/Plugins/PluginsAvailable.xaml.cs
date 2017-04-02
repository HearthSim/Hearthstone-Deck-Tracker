#region

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
		}

		private ObservableCollection<Plugin> _availablePlugins;
		private bool _loaded;

		private void GroupBox_Loaded(object sender, RoutedEventArgs e)
		{
			var dir = PluginManager.PluginDirectory;
			if(_loaded)
				return;
			try
			{
				_loaded = true;
				_availablePlugins = new ObservableCollection<Plugin>(InstallUtils.Instance.Plugins.Where(
					p => Directory.GetFiles(dir.FullName, p.Binary, SearchOption.AllDirectories)
						     .FirstOrDefault() == null).ToList());
				ListBoxAvailable.ItemsSource = _availablePlugins;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private async void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
		{
			var plugin = (Plugin)ListBoxAvailable.SelectedItem;
			if(plugin == null)
				return;
			
			//Localization: should use key SplashScreen_Text_Installing
			ButtonDownload.Content = "INSTALLING...";
			ButtonDownload.IsEnabled = false;
			
			if(await InstallUtils.InstallRemote(plugin))
			{
				//sync plugins
				var newPlugin = PluginManager.Instance.LoadPlugins(PluginManager.Instance.SyncPlugins()).FirstOrDefault();

				Core.MainWindow.ShowMessage($"Successfully installed {plugin.Name}", "").Forget();

				//Update Download Button 
				ButtonDownload.IsEnabled = true;
				ButtonDownload.Content = "INSTALL";

				_availablePlugins.Remove(plugin);
				GroupBoxAvailable.IsEnabled = false;

				//update plugin updatable
				var newPluginManagerItem = PluginManager.Instance.Plugins.First(x => x == newPlugin);
				//newPluginManagerItem.TempPlugin = plugin;
				InstallUtils.UpdateHyperlink(newPluginManagerItem);

				//switch to the window, scroll down and select plugin
				OptionsPluginsInstalled.Instance.ListBoxPlugins.SelectedItem = newPlugin;
				OptionsPluginsInstalled.Instance.ListBoxPlugins.ScrollIntoView(
					OptionsPluginsInstalled.Instance.ListBoxPlugins.SelectedItem);
				Helper.OptionsMain.ContentControlOptions.Content = Helper.OptionsMain.OptionsPluginsInstalled;
			}
			else
			{
				GithubUnavailable($"Unable to install {plugin.Name}.", plugin).Forget();
				ButtonDownload.IsEnabled = true;
				ButtonDownload.Content = "INSTALL";
			}
		}

		private async Task GithubUnavailable(string messageText, Plugin plugin)
		{
			if(InstallUtils.GithubRateLeft() > 0)
			{
				messageText = "You have hit GitHub's rate limit.";
			}
			var result = await Core.MainWindow.ShowMessageAsync("Unable to download plugin.",
					$"{messageText}\nTry again in {InstallUtils.GithubRateLeft()} or manually drag-and-drop the plugin.\nGo to the manual download page now?", MessageDialogStyle.AffirmativeAndNegative);
			if(result == MessageDialogResult.Negative || plugin == null)
				return;
			var releaseUrl = plugin.ReleaseUrl.Replace("api.", "").Replace("/repos", "");
			Helper.TryOpenUrl(releaseUrl);
		}

		private void ButtonDetails_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedItem = (Plugin)ListBoxAvailable.SelectedItem;
			if(selectedItem == null)
				return;
			var url =
				selectedItem.ReleaseUrl.Replace("api.", "").Replace("repos/", "").Replace(
					"/releases/latest", "") + "#readme";
			Helper.TryOpenUrl(url);
		}

		private void listBoxAvailable_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			GroupBoxAvailable.IsEnabled = true;
		}

		private void availablePlugins_Click(object sender, RoutedEventArgs e)
		{
			Helper.TryOpenUrl("https://github.com/HearthSim/Hearthstone-Deck-Tracker/wiki/Available-Plugins");
		}
	}
}