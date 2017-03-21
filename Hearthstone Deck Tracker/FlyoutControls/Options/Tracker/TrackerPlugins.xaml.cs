#region

using System;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerPlugins.xaml
	/// </summary>
	public partial class TrackerPlugins
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
				ListBoxAvailable.ItemsSource = InstallUtils.GetPlugins(ListBoxPlugins);
				_loaded = true;
				ListBoxUpdates.ItemsSource = InstallUtils.GetUpdates(ListBoxPlugins);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private void PluginsFolder()
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

		private async void GroupBox_Drop(object sender, DragEventArgs e)
		{
			if(!e.Data.GetDataPresent(DataFormats.FileDrop))
				return;
			if(InstallUtils.InstallPluginFile((string[])e.Data.GetData(DataFormats.FileDrop)))
			{
				var result = await Core.MainWindow.ShowMessageAsync($"Successfully installed the plugin(s).", "Restart now to take effect?", MessageDialogStyle.AffirmativeAndNegative);
				if(result == MessageDialogResult.Affirmative)
				{
					Core.MainWindow.Restart();
				}
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


		private async void ButtonUpdate_OnClick(object sender, RoutedEventArgs e)
		{
			var plugin = ListBoxUpdates.SelectedItem as Plugin;
			if(plugin == null)
			{
				Log.Error("Update pressed without a plugin selected.");
				return;
			}
			if(InstallUtils.UpdatePlugin(plugin))
			{
				var result = await Core.MainWindow.ShowMessageAsync($"Successfully updated {plugin.Name}.", "Restart now to take effect?", MessageDialogStyle.AffirmativeAndNegative);
				if(result == MessageDialogResult.Affirmative)
				{
					Core.MainWindow.Restart();
				}
			}
			else
			{
				GithubUnavailable($"Unable to update {plugin.Name}", plugin).Forget();
			}
		}

		
		private void ButtonUninstall_OnClick(object sender, RoutedEventArgs e)
		{
			var plugin = ListBoxPlugins.SelectedItem as PluginWrapper;
			if(plugin == null)
				return;
			if(InstallUtils.UninstallPlugin(plugin))
			{
				Core.MainWindow.ShowMessageAsync($"Deleted {plugin.Name}", "The plugin will be removed upon next restart.").Forget();
			}
			else
			{
				var result = Core.MainWindow.ShowMessageAsync($"Unable to delete {plugin.Name}", "Open the plugins folder to manually delete?", MessageDialogStyle.AffirmativeAndNegative).Result;
				if(result == MessageDialogResult.Negative)
					return;
				ButtonOpenPluginsFolder_OnClick(sender, e);
			}
		}

		private async void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
		{
			var plugin = ListBoxAvailable.SelectedItem as Plugin;
			if(plugin == null)
				return;
			if(InstallUtils.InstallRemote(plugin))
			{
				var result = await Core.MainWindow.ShowMessageAsync($"Successfully Installed {plugin.Name}.", "Restart now to take effect?", MessageDialogStyle.AffirmativeAndNegative);
				if(result == MessageDialogResult.Affirmative)
				{
					Core.MainWindow.Restart();
				}
			}
			else
			{
				GithubUnavailable($"Unable to install {plugin.Name}.", plugin).Forget();
			}
		}

		private async Task GithubUnavailable(string messageText, Plugin plugin)
		{
			if(InstallUtils.RateLimitHit())
			{
				messageText = "You have hit GitHub's rate limit.";
			}
			var result = await Core.MainWindow.ShowMessageAsync("Unable to download plugin.",
					$"{messageText}\nTry again later or manually drag-and-drop the plugin.\nGo to the manual download page now?", MessageDialogStyle.AffirmativeAndNegative);
			if(result == MessageDialogResult.Negative || plugin == null)
				return;
			var releaseUrl = plugin.ReleaseUrl.Replace("api.", "").Replace("/repos", "");
			Helper.TryOpenUrl(releaseUrl);
		}
	}
}