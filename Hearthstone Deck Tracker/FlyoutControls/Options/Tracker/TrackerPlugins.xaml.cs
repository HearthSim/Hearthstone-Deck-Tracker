#region

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Windows;

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
			var selected = ListBoxPlugins.SelectedItem as PluginWrapper;
			if(selected == null)
				return;
			selected.OnButtonPress();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
		}

		private void ButtonAvailablePlugins_OnClick(object sender, RoutedEventArgs e)
		{
			Process.Start(@"https://github.com/Epix37/Hearthstone-Deck-Tracker/wiki/Available-Plugins");
		}

		private void ButtonOpenPluginsFolder_OnClick(object sender, RoutedEventArgs e)
		{
			var pluginDir = Path.Combine(Environment.CurrentDirectory, "Plugins");
			if(!Directory.Exists(pluginDir))
			{
				try
				{
					Directory.CreateDirectory(pluginDir);
				}
				catch(Exception)
				{
					Helper.MainWindow.ShowMessage("Error",
					                              "Plugins directory not found and can not be created. Please manually create it in the Hearthstone Deck Tracker folder.");
					return;
				}
			}
            Process.Start(pluginDir);
		}
	}
}