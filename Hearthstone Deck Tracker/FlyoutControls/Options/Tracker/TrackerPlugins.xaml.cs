#region

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

		private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
		{
			(ListBoxPlugins.SelectedItem as PluginWrapper)?.OnButtonPress();
		}

		private async void GroupBox_Drop(object sender, DragEventArgs e)
		{
			var dir = PluginManager.PluginDirectory.FullName;
			try
			{
				if(e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					
					var plugins = 0;
					var droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
					if(droppedFiles == null) 
						return;
					foreach(var pluginPath in droppedFiles)
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
							if (Directory.GetDirectories(path).Length == 1 && Directory.GetFiles(path).Length == 0)
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
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				Core.MainWindow.ShowMessage("Error Importing Plugin", $"Please import manually to {dir}.").Forget();
			}
		}

		#endregion



		#region JSON functions

		private string GetAuthor(string apiUrl)
		{
			var wc = new WebClient();
			wc.Headers["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim";
			var json = JObject.Parse(wc.DownloadString(apiUrl));
			return json["owner"]["login"].ToString();
		}

		private string GetVersion(string releaseUrl)
		{
			var wc = new WebClient();
			wc.Headers["User-Agent"] = $"Hearthstone Deck Tracker {Core.Version} @ Hearthsim";
			var json = JObject.Parse(wc.DownloadString(releaseUrl));
			return json["tag_name"].ToString().Replace("v", "");
		}


		#endregion



		#region Available



		#endregion
		private void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
		{
			
		}
	}

	public class Plugin
	{
		private string Name { get; set; }
		private string NameAndVersion { get; set; }
		private string Author { get; set; }
		private string Description { get; set; }
		private string ReleaseUrl { get; set; }
	}
}