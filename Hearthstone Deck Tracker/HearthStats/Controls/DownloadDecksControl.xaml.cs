#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.Controls
{
	/// <summary>
	/// Interaction logic for DownloadDecksControl.xaml
	/// </summary>
	public partial class DownloadDecksControl : UserControl
	{
		private bool _done;

		public DownloadDecksControl()
		{
			InitializeComponent();
		}

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
			Helper.MainWindow.FlyoutHearthStatsDownload.IsOpen = false;
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

			var deleted = await HearthStatsSync.DeleteDeckAsync(deck);
			if(deleted)
				ListViewHearthStats.Items.Remove(deck);
		}
	}
}