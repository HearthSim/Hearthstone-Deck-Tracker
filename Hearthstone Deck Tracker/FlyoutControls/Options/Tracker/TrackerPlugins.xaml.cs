using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Plugins;

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
			var selected = ListBoxPlugins.SelectedItem as PluginWrapper;
			if(selected == null)
				return;
			selected.OnButtonPress();
		}

		private void ButtonReloadPlugins_OnClick(object sender, RoutedEventArgs e)
		{
			//Old plugins will still be loaded, but won't be called on "update" any longer
			PluginManager.Instance.LoadPlugins();
			ListBoxPlugins.ItemsSource = PluginManager.Instance.Plugins;
		}
	}
}
