#region

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
	public partial class OptionsPluginsInstalled
	{
		public OptionsPluginsInstalled()
		{
			InitializeComponent();
		}

		private bool _loaded;

		private async void GroupBox_Loaded(object sender, RoutedEventArgs e)
		{
			if(_loaded)
			{
				UpdateAppearance();
				return;
			}
			try
			{
				_loaded = true;
				foreach(var item in ListBoxPlugins.Items)
				{
					
					var plugin = item as PluginWrapper;
					var update = await InstallUtils.GetUpdate(plugin);
					if(update == null || plugin == null)
						 continue;
					plugin.TempPlugin = update.Plugin;

					if(!update.IsUpdatable)
					{
						plugin.UpdateHyperlink = "";
						plugin.UpdateTextDecorations = "None";
						plugin.UpdateTextEnabled = "False";
						continue;
					}
					if(update.IsUpToDate)
					{
						plugin.UpdateHyperlink = "Up to date ✔️";
						plugin.UpdateTextDecorations = "None";
						plugin.UpdateTextEnabled = "False";
						continue;
					}
					if(!update.IsUpToDate)
					{
						plugin.UpdateHyperlink = "Update available";
						plugin.UpdateTextDecorations = "Underline";
						plugin.UpdateTextEnabled = "True";
						continue;
					}
					UpdateAppearance();
				}
			} 
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private static void PluginsFolder()
		{
			var dir = PluginManager.PluginDirectory;
			if(!dir.Exists)
			{
				try
				{
					dir.Create();
				}
				catch(Exception)
				{
					Core.MainWindow.ShowMessage("Error",
						$"Plugins directory was not found and can not be created. Please manually create a folder called 'Plugins' under {dir}.").Forget();
				}
			}
			Helper.TryOpenUrl(dir.FullName);
		}

		private void ButtonOpenPluginsFolder_OnClick(object sender, RoutedEventArgs e)
		{
			PluginsFolder();
		}

		public void Load()
		{
			ListBoxPlugins.ItemsSource = PluginManager.Instance.Plugins;
			if(ListBoxPlugins.Items.Count > 0)
				ListBoxPlugins.SelectedIndex = 0;
			else
				GroupBoxDetails.Visibility = Visibility.Hidden;
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

		private async void GroupBox_Drop(object sender, DragEventArgs e)
		{
			if(!e.Data.GetDataPresent(DataFormats.FileDrop))
				return;
			if(InstallUtils.InstallPluginFile((string[])e.Data.GetData(DataFormats.FileDrop)))
			{
				var newPlugins = PluginManager.Instance.SyncPlugins();
				PluginManager.Instance.LoadPlugins(newPlugins);
			}
			else
			{
				var result = await Core.MainWindow.ShowMessageAsync($"Unable to install the plugin(s).", "Manually install through the plugins folder?", MessageDialogStyle.AffirmativeAndNegative);
				if(result == MessageDialogResult.Affirmative)
				{
					PluginsFolder();
				}
			}
		}
		
		private async void ButtonUninstall_OnClick(object sender, RoutedEventArgs e)
		{
			var plugin = ListBoxPlugins.SelectedItem as PluginWrapper;
			if(plugin == null)
				return;
			var ask = await Core.MainWindow.ShowMessageAsync($"Are you sure you want to remove {plugin.Name}?", "You may lose plugin settings.", MessageDialogStyle.AffirmativeAndNegative);
			if(ask == MessageDialogResult.Negative)
				return;

			if(InstallUtils.UninstallPlugin(plugin))
			{
				PluginManager.Instance.Plugins.Remove(plugin);
				plugin.Unload();
				plugin.IsEnabled = false;
			}
			else
			{
				var result = await Core.MainWindow.ShowMessageAsync($"Unable to delete {plugin.Name}", "Open the plugins folder to manually delete?", MessageDialogStyle.AffirmativeAndNegative);
				if(result == MessageDialogResult.Negative)
					return;
				ButtonOpenPluginsFolder_OnClick(sender, e);
			}
		}

		private static async Task GithubUnavailable(string messageText, Plugin plugin)
		{
			var rateLeft = InstallUtils.GithubRateLeft();
			if(rateLeft > 0)
			{
				messageText = "Too many requests.";
			}
			var dialogSettings = new MetroDialogSettings
			{
				AffirmativeButtonText = "Go to Download",
				NegativeButtonText = "Try again later"
			};
			var result = await Core.MainWindow.ShowMessageAsync("Unable to download plugin.",
					$"{messageText}\nTry again in {TimeSpan.FromSeconds(rateLeft).Minutes + 1} minutes or manually download and install the plugin." +
					$"\nGo to the manual download page now?", MessageDialogStyle.AffirmativeAndNegative, dialogSettings);
			if(result == MessageDialogResult.Negative || plugin == null)
				return;
			var releaseUrl = plugin.ReleaseUrl.Replace("api.", "").Replace("/repos", "");
			Helper.TryOpenUrl(releaseUrl);
		}

		private void UpdateAppearance()
		{
			foreach(var p in ListBoxPlugins.Items)
			{
				var color = "";
				var plugin = p as PluginWrapper;
				if(plugin == null)
					continue;
				switch(plugin.UpdateHyperlink)
				{
					case "":
						break;
					case "Up to date ✔️":
						color = "#808080"; // light green
						break;
					case "Update Available":
						color = "#FF47B1DF"; // Metro blue 
						break;
					default:
						color = "#FF47B1DF";
						break;
				}
				plugin.UpdateTextColor = p == ListBoxPlugins.SelectedItem ? "White" : color;
			}
		}
		private void ListBoxPlugins_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateAppearance();

		private async void UpdateLink_Click(object sender, RoutedEventArgs e)
		{
			var pluginWrapper = (PluginWrapper)((Hyperlink) sender).DataContext;

			if(pluginWrapper.UpdateHyperlink == "Update Installed. Restart now?")
			{
				Core.MainWindow.Restart();
				return;
			}

			pluginWrapper.UpdateTextEnabled = "False";
			pluginWrapper.UpdateHyperlink = "Updating...";

			if (string.IsNullOrEmpty(pluginWrapper.Repourl))
				return;
			if(await InstallUtils.UpdatePlugin(pluginWrapper.TempPlugin))
			{
				pluginWrapper.UpdateHyperlink = "Update Installed. Restart now?";
				pluginWrapper.UpdateTextEnabled = "True";
			}
			else
			{
				pluginWrapper.UpdateHyperlink = "Update failed. Retry?";
				pluginWrapper.UpdateTextEnabled = "True";
				GithubUnavailable($"Unable to update {pluginWrapper.Name}", pluginWrapper.TempPlugin).Forget();
			}
		}
	}
}