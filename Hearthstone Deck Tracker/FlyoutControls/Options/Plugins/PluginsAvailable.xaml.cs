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

		private bool _loaded;

		private void GroupBox_Loaded(object sender, RoutedEventArgs e)
		{
			if(_loaded)
				return;
			try
			{
				ListBoxAvailable.ItemsSource = InstallUtils.GetPlugins(PluginManager.Instance.Plugins);
				_loaded = true;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		/*
		private void ButtonReadme_OnClick(object sender, RoutedEventArgs e)
		{
			var releases = (ListBoxPlugins.SelectedItem as PluginWrapper)?.Repourl + "#readme";
			Helper.TryOpenUrl(releases);
		}
		*/

		private void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
		{
			var plugin = ListBoxAvailable.SelectedItem as Plugin;
			if(plugin == null)
				return;
			if(InstallUtils.InstallRemote(plugin))
			{
				var newPlugins = PluginManager.Instance.SyncPlugins();
				PluginManager.Instance.LoadPlugins(newPlugins);
				Core.MainWindow.ShowMessage($"Successfully installed {plugin.Name}", "").Forget();
			}
			else
			{
				GithubUnavailable($"Unable to install {plugin.Name}.", plugin).Forget();
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
	}
}