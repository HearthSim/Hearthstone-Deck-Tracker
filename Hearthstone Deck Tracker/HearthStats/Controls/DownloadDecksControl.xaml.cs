#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.Controls
{
	/// <summary>
	/// Interaction logic for DownloadDecksControl.xaml
	/// </summary>
	public partial class DownloadDecksControl : UserControl
	{
		private bool _done;
		private List<Deck> _selectedDecks;

		public DownloadDecksControl()
		{
			InitializeComponent();
		}

		public async Task<List<Deck>> LoadDecks(IEnumerable<Deck> decks)
		{
			_selectedDecks = decks.ToList();

			ListViewHearthStats.Items.Clear();
			foreach(var deck in _selectedDecks)
				ListViewHearthStats.Items.Add(deck);

			_done = false;
			while(!_done)
				await Task.Delay(100);

			return _selectedDecks;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_selectedDecks = ListViewHearthStats.Items.Cast<Deck>().ToList();
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
			var result =
				await
				Helper.MainWindow.ShowMessageAsync("Delete " + deck.Name,
				                                   "This will permanentely delete the deck and all associated stats. Are you sure?",
				                                   MessageDialogStyle.AffirmativeAndNegative,
				                                   new MetroDialogSettings {AffirmativeButtonText = "delete", NegativeButtonText = "cancel"});
			if(result == MessageDialogResult.Affirmative)
			{
				var deleted = await HearthStatsManager.DeleteDeckAsync(deck, false, true);
				if(deleted)
					ListViewHearthStats.Items.Remove(deck);
			}
		}

		private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
		{
			_selectedDecks = new List<Deck>();
			_done = true;
			Helper.MainWindow.FlyoutHearthStatsDownload.IsOpen = false;
		}
	}
}