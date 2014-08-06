using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for GameStats.xaml
	/// </summary>
	public partial class DeckStatsControl
	{
		private Deck _deck;

		public DeckStatsControl()
		{
			InitializeComponent();
		}

		private async void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			if(_deck == null)
				return;

			var selectedGame = DGrid.SelectedItem as GameStats;
			if(selectedGame == null)
				return;

			if(await Helper.MainWindow.ShowDeleteGameStatsMessage(selectedGame) != MessageDialogResult.Affirmative)
				return;

			if(_deck.DeckStats.Games.Contains(selectedGame))
			{
				_deck.DeckStats.Games.Remove(selectedGame);
				DeckStatsList.Save();
				Helper.MainWindow.DeckPickerList.Items.Refresh();
				Refresh();
			}
		}

		public void SetDeck(Deck deck, bool refreshSort = true)
		{
			_deck = deck;

			DGrid.Items.Clear();
			foreach(var game in deck.DeckStats.Games)
				DGrid.Items.Add(game);
			DataGridWinLoss.Items.Clear();
			DataGridWinLoss.Items.Add(new WinLoss(deck.DeckStats.Games, "Win"));
			DataGridWinLoss.Items.Add(new WinLoss(deck.DeckStats.Games, "Loss"));

			if(refreshSort)
			{
				DGrid.Items.SortDescriptions.Clear();
				DGrid.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));
			}
		}

		public void Refresh()
		{
			if(_deck != null)
				SetDeck(_deck, false);
		}

		private class WinLoss
		{
			private readonly List<GameStats> _stats;

			public WinLoss(List<GameStats> stats, string result)
			{
				_stats = stats;
				Result = result;
			}

			public string Result { get; private set; }

			public string Total
			{
				get
				{
					var numGames = _stats.Count(s => s.Result.ToString() == Result);
					return GetDisplayString(numGames, _stats.Count);
				}
			}

			public string Druid
			{
				get { return GetClassDisplayString("Druid"); }
			}

			public string Hunter
			{
				get { return GetClassDisplayString("Hunter"); }
			}

			public string Mage
			{
				get { return GetClassDisplayString("Mage"); }
			}

			public string Paladin
			{
				get { return GetClassDisplayString("Paladin"); }
			}

			public string Priest
			{
				get { return GetClassDisplayString("Priest"); }
			}

			public string Rogue
			{
				get { return GetClassDisplayString("Rogue"); }
			}

			public string Shaman
			{
				get { return GetClassDisplayString("Shaman"); }
			}

			public string Warrior
			{
				get { return GetClassDisplayString("Warrior"); }
			}

			public string Warlock
			{
				get { return GetClassDisplayString("Warlock"); }
			}

			private string GetClassDisplayString(string hsClass)
			{
				var numGames = _stats.Count(s => s.Result.ToString() == Result && s.OpponentHero == hsClass);
				var total = _stats.Count(s => s.OpponentHero == hsClass);
				return GetDisplayString(numGames, total);
			}

			private string GetDisplayString(int num, int total)
			{
				var percent = total > 0 ? Math.Round(100.0 * num / total, 2).ToString() : "-";
				return string.Format("{0} ({1}%)", num, percent);
			}
		}
	}
}