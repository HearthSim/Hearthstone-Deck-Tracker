#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class GameStatsHelper
	{
		public static void MoveGamesToOtherDeckWithDialog(DependencyObject parent, params GameStats[] games)
		{
			if(games == null)
				return;
			var window = Helper.GetParentWindow(parent);
			if(window == null)
				return;
			var heroes = new Dictionary<string, int>();
			foreach(var game in games)
			{
				if(!heroes.ContainsKey(game.PlayerHero))
					heroes.Add(game.PlayerHero, 0);
				heroes[game.PlayerHero]++;
			}
			var heroPlayed = heroes.Any() ? heroes.OrderByDescending(x => x.Value).First().Key : "Any";
			var possibleTargets = DeckList.Instance.Decks.Where(d => d.Class == heroPlayed || heroPlayed == "Any");
			var dialog = new MoveGameDialog(possibleTargets) {Owner = window};
			dialog.ShowDialog();
			if(dialog.SelectedDeck == null)
				return;
			MoveGamesToOtherDeckWithoutConfirmation(dialog.SelectedDeck, dialog.SelectedVersion, games);
		}

		internal static void MoveGamesToOtherDeckWithoutConfirmation(Deck targetDeck, SerializableVersion targetVersion,
																	 params GameStats[] games)
		{
			if(games == null)
				return;
			foreach(var game in games)
			{
				var defaultDeck = DefaultDeckStats.Instance.DeckStats.FirstOrDefault(ds => ds.Games.Contains(game));
				if(defaultDeck != null)
				{
					defaultDeck.Games.Remove(game);
					DefaultDeckStats.Save();
				}
				else
				{
					var deck = DeckList.Instance.Decks.FirstOrDefault(d => game.DeckId == d.DeckId);
					deck?.DeckStats.Games.Remove(game);
				}
				game.PlayerDeckVersion = targetVersion;
				game.HearthStatsDeckVersionId = targetDeck.GetVersion(targetVersion).HearthStatsDeckVersionId;
				game.DeckId = targetDeck.DeckId;
				game.DeckName = targetDeck.Name;
				targetDeck.DeckStats.Games.Add(game);
				if(HearthStatsAPI.IsLoggedIn && Config.Instance.HearthStatsAutoUploadNewGames)
					HearthStatsManager.MoveMatchAsync(game, targetDeck, background: true).Forget();
			}
			DeckStatsList.Save();
			DeckList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		public static async Task DeleteGamesWithDialog(DependencyObject control, params GameStats[] games)
		{
			games = games.Where(x => x != null).ToArray();
			if(games.Length == 0)
				return;
			var window = Helper.GetParentWindow(control);
			if(window == null)
				return;
			if(games.Length == 1 && await window.ShowDeleteGameStatsMessage(games.Single()) != MessageDialogResult.Affirmative)
				return;
			if(games.Length > 1 && await window.ShowDeleteMultipleGameStatsMessage(games.Length) != MessageDialogResult.Affirmative)
				return;
			await DeleteGamesWithoutConfirmation(games);
		}

		internal static async Task DeleteGamesWithoutConfirmation(params GameStats[] games)
		{
			games = games.Where(x => x != null).ToArray();
			if(games.Length == 0)
				return;
			var saveDeckStats = false;
			var saveDefaultDeckStats = false;
			foreach(var game in games)
			{
				var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckStats.Games.Contains(game));
				if(deck != null)
				{
					if(deck.DeckStats.Games.Contains(game))
					{
						game.DeleteGameFile();
						deck.DeckStats.Games.Remove(game);
						Log.Info($"Deleted game {game} from {deck}.");
						saveDeckStats = true;
					}
				}
				else
				{
					var deckstats = DefaultDeckStats.Instance.DeckStats.FirstOrDefault(ds => ds.Games.Contains(game));
					if(deckstats != null)
					{
						game.DeleteGameFile();
						deckstats.Games.Remove(game);
						Log.Info($"Deleted game {game} from default deck.");
						saveDefaultDeckStats = true;
					}
				}
			}

			if(HearthStatsAPI.IsLoggedIn && games.Any(g => g.HasHearthStatsId)
			   && await Core.MainWindow.ShowCheckHearthStatsMatchDeletionDialog())
				HearthStatsManager.DeleteMatchesAsync(games.ToList()).Forget();
			if(saveDeckStats)
				DeckStatsList.Save();
			if(saveDefaultDeckStats)
				DefaultDeckStats.Save();
			Log.Info($"Deleted {games.Length} games");
			Core.MainWindow.DeckPickerList.UpdateDecks();
			ConstructedStats.Instance.UpdateConstructedStats();
		}
	}
}