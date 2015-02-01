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
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.HearthStats.Controls
{
	/// <summary>
	/// Interaction logic for DownloadDecksControl.xaml
	/// </summary>
	public partial class DownloadDecksControl : UserControl
	{
		public DownloadDecksControl()
		{
			InitializeComponent();
		}

		private bool _done;

		public async Task<List<Deck>> LoadLocalDecks(IEnumerable<Deck> decks)
		{
			foreach(var deck in decks)
				ListViewHearthStats.Items.Add(deck);

			_done = false;
			while(!_done)
				await Task.Delay(100);

			return ListViewHearthStats.Items.Cast<Deck>().ToList();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_done = true;
		}

		private async void BtnDeleteRemoteDeck_OnClick(object sender, RoutedEventArgs e)
		{
			var btn = sender as Button;
			if(btn == null)
				return;
			var deck = btn.DataContext as Deck;
			if(deck == null)
				return;

			//show warning

			var deleted = await HearthStatsSync.DeleteDeck(deck);
			if(deleted)
			{
				ListViewHearthStats.Items.Remove(deck);
			}
			else
			{
				//error message
			}
		}
	}
}
