#region

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
	public partial class TrackerPlugins : UserControl
	{
		public TrackerPlugins()
		{
			InitializeComponent();
		}

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

		private void ButtonAvailablePlugins_OnClick(object sender, RoutedEventArgs e) => Helper.TryOpenUrl(@"https://github.com/HearthSim/Hearthstone-Deck-Tracker/wiki/Available-Plugins");

		private void ButtonOpenPluginsFolder_OnClick(object sender, RoutedEventArgs e)
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

		private async void GroupBox_Drop(object sender, DragEventArgs e)
		{
			var dir = PluginManager.PluginDirectory.FullName;
			int plugins = 0;
			try
			{
				if(e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
					if (droppedFiles == null) return;
					foreach(var pluginPath in droppedFiles)
					{
						if (!pluginPath.EndsWith(".dll")) continue;
						File.Copy(pluginPath, dir + "\\" + Path.GetFileName(pluginPath), true);
						plugins += 1;
					}
					if (plugins > 0)
					{
						var result = await Core.MainWindow.ShowMessageAsync("Plugins installed",
							$"Successfully installed {plugins} plugins. \n Restart now to take effect?", MessageDialogStyle.AffirmativeAndNegative);

						if (result == MessageDialogResult.Affirmative)
						{
							Core.MainWindow.Restart();
						}
					}
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				Core.MainWindow.ShowMessage("Error Importing Plugin", $"Please import manually to {dir}.").Forget();
			}
		}
	}
}