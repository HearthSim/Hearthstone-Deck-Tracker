using Hearthstone_Deck_Tracker.Hearthstone;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public partial class MainWindowMenuView : UserControl
	{
		public MainWindowMenuView()
		{
			InitializeComponent();
		}

		public IEnumerable<Deck> SelectedDecks
		{
			get => ((MainWindowMenuViewModel)DataContext).Decks;
			set => ((MainWindowMenuViewModel)DataContext).Decks = value;
		}

		private void Plugins_OnSubmenuOpened(object sender, RoutedEventArgs e)
		{
			((MainWindowMenuViewModel)DataContext).PluginsMenuOpened();
		}

		private void Deck_OnSubmenuOpened(object sender, RoutedEventArgs e)
		{
			((MainWindowMenuViewModel)DataContext).DeckMenuOpened();
		}
	}
}
