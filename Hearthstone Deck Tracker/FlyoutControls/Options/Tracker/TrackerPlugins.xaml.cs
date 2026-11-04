#region

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility;
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
					this.ParentMainWindow()?.ShowMessage("Error",
												$"Plugins directory was not found and can not be created. Please manually create a folder called 'Plugins' under {dir}.").Forget();
				}
			}
			Helper.TryOpenUrl(dir.FullName);
		}

		private async void DockPanel_Drop(object sender, DragEventArgs e)
		{
			if(!e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetData(DataFormats.FileDrop) is not string[] droppedFiles)
				return;
			var pluginFiles = droppedFiles.Where(x => x.EndsWith(".dll") || x.EndsWith(".zip")).ToList();
			if(pluginFiles.Count == 0 || this.ParentMainWindow() is not { } window)
				return;

			var result = await window.ShowMessageAsync(
				string.Format(LocUtil.Get("Options_Tracker_Plugins_Dialog_Install_Title"), string.Join(", ", pluginFiles.Select(Path.GetFileName))),
				LocUtil.Get("Options_Tracker_Plugins_Dialog_Install_Desc"),
				MessageDialogStyle.AffirmativeAndNegative,
				new MessageDialogs.Settings
				{
					AffirmativeButtonText = LocUtil.Get("Options_Tracker_Plugins_Dialog_Install_Button_Install"),
					NegativeButtonText = LocUtil.Get("Options_Tracker_Plugins_Dialog_Install_Button_Cancel"),
					DefaultButtonFocus = MessageDialogResult.Negative
				});
			if(result != MessageDialogResult.Affirmative)
				return;

			var dir = PluginManager.PluginDirectory.FullName;
			try
			{
				foreach(var pluginPath in pluginFiles)
				{
					if(pluginPath.EndsWith(".dll"))
						File.Copy(pluginPath, Path.Combine(dir, Path.GetFileName(pluginPath)), true);
					else
					{
						var path = Path.Combine(dir, Path.GetFileNameWithoutExtension(pluginPath));
						ZipFile.ExtractToDirectory(pluginPath, path);
						if (Directory.GetDirectories(path).Length == 1 && Directory.GetFiles(path).Length == 0)
						{
							Directory.Delete(path, true);
							ZipFile.ExtractToDirectory(pluginPath, dir);
						}
					}
				}
				PluginManager.Instance.LoadPlugins(PluginManager.Instance.SyncPlugins());
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				this.ParentMainWindow()?.ShowMessage("Error Importing Plugin", $"Please import manually to {dir}.").Forget();
			}
		}
	}
}
