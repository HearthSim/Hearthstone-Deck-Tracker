#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.Controls;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class DataIssueResolver
	{
		public static void Run()
		{
			if(!Config.Instance.RemovedNoteUrls)
				RemoveNoteUrls();
			if(!Config.Instance.ResolvedDeckStatsIssue)
				ResolveDeckStatsIssue();
			if(!Config.Instance.FixedDuplicateMatches)
				RemoveDuplicateMatches(false);
			if(!Config.Instance.ResolvedOpponentNames)
				ResolveOpponentNames();
			if(!Config.Instance.ResolvedDeckStatsIds)
				ResolveDeckStatsIds();
		}


		internal static async void RemoveDuplicateMatches(bool showDialogIfNoneFound)
		{
			try
			{
				Log.Info("Checking for duplicate matches...");
				var toRemove = new Dictionary<GameStats, List<GameStats>>();
				foreach(var deck in DeckList.Instance.Decks)
				{
					var duplicates =
						deck.DeckStats.Games.Where(x => !string.IsNullOrEmpty(x.OpponentName))
						    .GroupBy(g => new {g.OpponentName, g.Turns, g.PlayerHero, g.OpponentHero, g.Rank});
					foreach(var games in duplicates)
					{
						if(games.Count() > 1)
						{
							var ordered = games.OrderBy(x => x.StartTime);
							var original = ordered.First();
							var filtered = ordered.Skip(1).Where(x => x.HasHearthStatsId).ToList();
							if(filtered.Count > 0)
								toRemove.Add(original, filtered);
						}
					}
				}
				if(toRemove.Count > 0)
				{
					var numMatches = toRemove.Sum(x => x.Value.Count);
					Log.Info(numMatches + " duplicate matches found.");
					var result =
						await
						Core.MainWindow.ShowMessageAsync("Detected " + numMatches + " duplicate matches.",
						                                 "Due to sync issues some matches have been duplicated, click \"fix now\" to see and delete duplicates. Sorry about this.",
						                                 MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
						                                 new MessageDialogs.Settings
						                                 {
							                                 AffirmativeButtonText = "fix now",
							                                 NegativeButtonText = "fix later",
							                                 FirstAuxiliaryButtonText = "don't fix"
						                                 });
					if(result == MessageDialogResult.Affirmative)
					{
						var dmw = new DuplicateMatchesWindow();
						dmw.LoadMatches(toRemove);
						dmw.Show();
					}
					else if(result == MessageDialogResult.FirstAuxiliary)
					{
						Config.Instance.FixedDuplicateMatches = true;
						Config.Save();
					}
				}
				else if(showDialogIfNoneFound)
					await Core.MainWindow.ShowMessageAsync("No duplicate matches found.", "");
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}


		private static void ResolveDeckStatsIds()
		{
			foreach(var deckStats in DeckStatsList.Instance.DeckStats)
			{
				var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.Name == deckStats.Name);
				if(deck != null)
				{
					deckStats.DeckId = deck.DeckId;
					deckStats.HearthStatsDeckId = deck.HearthStatsId;
				}
			}
			DeckStatsList.Save();
			DeckList.Save();
			Config.Instance.ResolvedDeckStatsIds = true;
			Config.Save();
		}

		private static void RemoveNoteUrls()
		{
			foreach(var deck in DeckList.Instance.Decks)
			{
				if(!string.IsNullOrEmpty(deck.Url))
					deck.Note = deck.Note.Replace(deck.Url, "").Trim();
			}
			DeckList.Save();
			Config.Instance.RemovedNoteUrls = true;
			Config.Save();
		}

		private static void ResolveDeckStatsIssue()
		{
			foreach(var deck in DeckList.Instance.Decks)
			{
				foreach(var deckVersion in deck.Versions)
				{
					if(deckVersion.DeckStats.Games.Any())
					{
						var games = deckVersion.DeckStats.Games.ToList();
						foreach(var game in games)
						{
							deck.DeckStats.AddGameResult(game);
							deckVersion.DeckStats.Games.Remove(game);
						}
					}
				}
			}
			foreach(var deckStats in DeckStatsList.Instance.DeckStats)
			{
				if(deckStats.Games.Any() && !DeckList.Instance.Decks.Any(d => deckStats.BelongsToDeck(d)))
				{
					var games = deckStats.Games.ToList();
					foreach(var game in games)
					{
						var defaultStats = DefaultDeckStats.Instance.GetDeckStats(game.PlayerHero);
						if(defaultStats != null)
						{
							defaultStats.AddGameResult(game);
							deckStats.Games.Remove(game);
						}
					}
				}
			}

			DeckStatsList.Save();
			Config.Instance.ResolvedDeckStatsIssue = true;
			Config.Save();
		}

		/// <summary>
		/// v0.10.0 caused opponent names to be saved as the hero, rather than the name.
		/// </summary>
		private static async void ResolveOpponentNames()
		{
			var games =
				DeckStatsList.Instance.DeckStats.SelectMany(ds => ds.Games)
				             .Where(g => g.HasReplayFile && Enum.GetNames(typeof(HeroClass)).Any(x => x == g.OpponentName))
				             .ToList();
			if(!games.Any())
			{
				Config.Instance.ResolvedOpponentNames = true;
				Config.Save();
				return;
			}
			var controller =
				await
				Core.MainWindow.ShowProgressAsync("Fixing opponent names in recorded games...",
				                                  "v0.10.0 caused opponent names to be set to their hero, rather than the actual name.\n\nThis may take a moment.\n\nYou can cancel to continue this at a later time (or not at all).",
				                                  true);

			await FixOppNameAndClass(games, controller);

			await controller.CloseAsync();
			if(controller.IsCanceled)
			{
				var fix =
					await
					Core.MainWindow.ShowMessageAsync("Cancelled", "Fix remaining names on next start?", MessageDialogStyle.AffirmativeAndNegative,
					                                 new MessageDialogs.Settings
					                                 {
						                                 AffirmativeButtonText = "next time",
						                                 NegativeButtonText = "don\'t fix"
					                                 });
				if(fix == MessageDialogResult.Negative)
				{
					Config.Instance.ResolvedOpponentNames = true;
					Config.Save();
				}
			}
			else
			{
				Config.Instance.ResolvedOpponentNames = true;
				Config.Save();
			}
			DeckStatsList.Save();
		}

		internal static async Task<int> FixOppNameAndClass(List<GameStats> games, ProgressDialogController controller)
		{
			var count = 0;
			var fixCount = 0;
			var gamesCount = games.Count;
			var lockMe = new object();
			var options = new ParallelOptions {MaxDegreeOfParallelism = Environment.ProcessorCount};
			await Task.Run(() =>
			{
				Parallel.ForEach(games, options, (game, loopState) =>
				{
					if(controller.IsCanceled)
					{
						loopState.Stop();
						return;
					}
					List<ReplayKeyPoint> replay;
					try
					{
						replay = ReplayReader.LoadReplay(game.ReplayFile);
					}
					catch
					{
						return;
					}
					var last = replay.LastOrDefault();
					if(last == null)
						return;
					var opponent = last.Data.FirstOrDefault(x => x.IsOpponent);
					if(opponent == null)
						return;
					var incremented = false;
					if(game.OpponentName != opponent.Name)
					{
						game.OpponentName = opponent.Name;
						Interlocked.Increment(ref fixCount);
						incremented = true;
					}
					var heroEntityId = opponent.GetTag(GAME_TAG.HERO_ENTITY);
					var entity = last.Data.FirstOrDefault(x => x.Id == heroEntityId);
					if(entity != null)
					{
						string hero;
						if(CardIds.HeroIdDict.TryGetValue(entity.CardId ?? "", out hero) && game.OpponentHero != hero)
						{
							game.OpponentHero = hero;
							if(!incremented)
								Interlocked.Increment(ref fixCount);
						}
					}
					lock(lockMe)
					{
						controller.SetProgress(1.0 * ++count / gamesCount);
					}
				});
			});
			return fixCount;
		}
	}
}