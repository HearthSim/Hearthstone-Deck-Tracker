#region

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;

#endregion

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

		private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			await PostDeckAsync(Helper.MainWindow.DeckPickerList.SelectedDeck);
			//HearthStatsAPI.PostDeck(Helper.MainWindow.DeckPickerList.GetSelectedDeckVersion());
		}

		private void BtnGetDecks_OnClick(object sender, RoutedEventArgs e)
		{
			//HearthStatsAPI.GetDecks(DateTime.MinValue);
			//HearthStatsAPI.GetDecks(DateTime.Today);
			//HearthStatsAPI.GetDecks(DateTime.Now.ToUnixTime());
		}

		private void BtnPostGame_OnClick(object sender, RoutedEventArgs e)
		{
			//var game = Helper.MainWindow.DeckPickerList.SelectedDeck.DeckStats.Games.FirstOrDefault();
			//HearthStatsAPI.PostGameResult(game, Helper.MainWindow.DeckPickerList.SelectedDeck);
		}

		private async void BtnPosFullDeck_OnClick(object sender, RoutedEventArgs e)
		{
			//var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
			//await PostDeckAsync(deck);
		}

		private async Task PostDeckAsync(Deck deck)
		{
			await HearthStatsManager.UploadDeckAsync(deck);
			//foreach(var game in deck.DeckStats.Games)
			//	await HearthStatsAPI.PostGameResultAsync(game, deck);
		}

		private void PostDeck(Deck deck)
		{
			HearthStatsAPI.PostDeck(deck);
			//Parallel.ForEach(deck.DeckStats.Games, game => HearthStatsAPI.PostGameResult(game, deck));
		}

		private void BtnPostAll_OnClick(object sender, RoutedEventArgs e)
		{
			var sw = Stopwatch.StartNew();

			var decks = Helper.MainWindow.DeckList.DecksList;
			var count = 0;

			Parallel.ForEach(decks, deck =>
			{
				PostDeck(deck);
				var progress = Interlocked.Increment(ref count);
				Console.WriteLine(progress + "/" + decks.Count);
			});

			Helper.MainWindow.WriteDecks();
			DeckStatsList.Save();

			Console.WriteLine(sw.ElapsedMilliseconds);
			sw.Stop();
		}
	}
}