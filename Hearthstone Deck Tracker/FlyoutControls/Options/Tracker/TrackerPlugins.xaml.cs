#region

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
		}

		private void ListBoxPlugins_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
		}

		private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
		{
			((sender as Control)?.DataContext as PluginWrapper)?.OnButtonPress();
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

		private void DockPanel_Drop(object sender, DragEventArgs e)
		{
			var dir = PluginManager.PluginDirectory.FullName;
			var prevCount = PluginManager.Instance.Plugins.Count;
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
					PluginManager.Instance.LoadPlugins(PluginManager.Instance.SyncPlugins());
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
