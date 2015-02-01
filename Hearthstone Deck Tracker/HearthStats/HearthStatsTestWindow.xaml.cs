using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.HearthStats
{
	/// <summary>
	/// Interaction logic for HearthStatsTestWindow.xaml
	/// </summary>
	public partial class HearthStatsTestWindow : Window
	{
		public HearthStatsTestWindow()
		{
			InitializeComponent();
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			HearthStatsAPI.PostDeck(Helper.MainWindow.DeckPickerList.GetSelectedDeckVersion());
		}

		private void BtnGetDecks_OnClick(object sender, RoutedEventArgs e)
		{
			//HearthStatsAPI.GetDecks(DateTime.MinValue);
			//HearthStatsAPI.GetDecks(DateTime.Today);
			HearthStatsAPI.GetDecks(DateTime.Now.ToUnixTime());
		}

		private void BtnPostGame_OnClick(object sender, RoutedEventArgs e)
		{
			var game = Helper.MainWindow.DeckPickerList.SelectedDeck.DeckStats.Games.FirstOrDefault();
			HearthStatsAPI.PostGameResult(game, Helper.MainWindow.DeckPickerList.SelectedDeck);
		}

		private async void BtnPosFullDeck_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
			await PostDeck(deck);

		}

		private async Task PostDeck(Deck deck)
		{
			await HearthStatsAPI.PostDeck(deck);
			foreach(var game in deck.DeckStats.Games)
				await HearthStatsAPI.PostGameResult(game, deck);

		}

		private async void BtnPostAll_OnClick(object sender, RoutedEventArgs e)
		{
			return;
			var sw = Stopwatch.StartNew();
			foreach(var deck in Helper.MainWindow.DeckList.DecksList)
				await PostDeck(deck);
            Helper.MainWindow.WriteDecks();
			DeckStatsList.Save();

			Console.WriteLine(sw.ElapsedMilliseconds);
			sw.Stop();
		}
	}
}
