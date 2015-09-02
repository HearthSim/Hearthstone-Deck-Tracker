#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.Controls
{
	/// <summary>
	/// Interaction logic for DuplicateMatchesWindow.xaml
	/// </summary>
	public partial class DuplicateMatchesWindow
	{
		public readonly List<GameStatsWrapper> _allWrappers = new List<GameStatsWrapper>();

		public DuplicateMatchesWindow()
		{
			InitializeComponent();
		}

		public void LoadMatches(Dictionary<GameStats, List<GameStats>> games)
		{
			try
			{
				_allWrappers.Clear();
				foreach(var set in games.OrderBy(x => x.Value.Count))
				{
					var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == set.Key.DeckId);
					var tvi = new TreeViewItem();
					tvi.ItemTemplate = (DataTemplate)FindResource("DataTemplateCheckBox");
					tvi.Header = string.Format("[Original - Deck: {0}] : {1} ({2} duplicate(s))", deck != null ? deck.Name : "", GetMatchInfo(set.Key),
					                           set.Value.Count);
					tvi.IsExpanded = true;
					foreach(var game in set.Value)
					{
						try
						{
							var wrapper = new GameStatsWrapper(game);
							tvi.Items.Add(wrapper);
							_allWrappers.Add(wrapper);
						}
						catch(Exception e)
						{
							Logger.WriteLine("Error loading duplicate match: " + e, "DuplicateMatchesWindow");
						}
					}
					TreeViewGames.Items.Add(tvi);
				}
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error loading duplicate matches: " + ex, "DuplicateMatchesWindow");
			}
		}

		public static string GetMatchInfo(GameStats game)
		{
			return string.Format("{0} vs {1} ({2}), {3}", game.Result, game.OpponentName, game.OpponentHero, game.StartTime);
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			var selected = _allWrappers.Where(x => x.ToDelete).ToList();
			if(selected.Any())
			{
				var matches = selected.Select(x => x.GameStats).ToList();
				Logger.WriteLine("Deleting " + matches.Count + " duplicate matches.");
				var controller = await this.ShowProgressAsync("Deleting duplicate matches...", "Deleting duplicates on HearthStats...");
				await HearthStatsManager.DeleteMatchesAsync(matches.ToList(), false);
				controller.SetMessage("Deleting local duplicates...");
				foreach(var match in matches)
				{
					var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == match.DeckId);
					if(deck == null)
						continue;
					deck.DeckStats.Games.Remove(match);
				}
				DeckStatsList.Save();
				Helper.MainWindow.DeckPickerList.UpdateDecks();
				await controller.CloseAsync();
			}
			await this.ShowMessageAsync("Success", "Deleted " + _allWrappers.Count(x => x.ToDelete) + " duplicates.");
			Config.Instance.FixedDuplicateMatches = true;
			Config.Save();
			Close();
		}

		private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
		{
			foreach(var wrapper in _allWrappers)
				wrapper.ToDelete = true;
		}

		private void ButtonDeselectAll_Click(object sender, RoutedEventArgs e)
		{
			foreach(var wrapper in _allWrappers)
				wrapper.ToDelete = false;
		}

		public class GameStatsWrapper : INotifyPropertyChanged
		{
			private bool _toDelete;

			public GameStatsWrapper(GameStats gameStats)
			{
				GameStats = gameStats;
				DisplayName = "[Duplicate] " + GetMatchInfo(gameStats);
				ToDelete = true;
			}

			public GameStats GameStats { get; set; }
			public string DisplayName { get; set; }

			public bool ToDelete
			{
				get { return _toDelete; }
				set
				{
					_toDelete = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			[NotifyPropertyChangedInvocator]
			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				var handler = PropertyChanged;
				if(handler != null)
					handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}