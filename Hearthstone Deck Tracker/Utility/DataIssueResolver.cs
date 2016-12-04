#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Enums;
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
		internal static bool RunDeckStatsFix;
		public static void Run()
		{
			if(Directory.Exists(GamesDir))
				InitiateGameFilesCleanup();
			if(RunDeckStatsFix)
				FixDeckStats();
			if(Directory.Exists("Images/Bars"))
				CleanUpImageFiles();
		}

		private static void CleanUpImageFiles()
		{
			Log.Info("Cleaning up old card image files...");
			try
			{
				Directory.Delete("Images/Bars", true);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			Log.Info("Cleanup complete.");
		}

		//https://github.com/HearthSim/Hearthstone-Deck-Tracker/issues/2675
		private static void FixDeckStats()
		{
			var save = false;
			foreach(var d in DeckList.Instance.Decks.Where(d => d.DeckStats.DeckId != d.DeckId))
			{
				DeckStats deckStats;
				if(!DeckStatsList.Instance.DeckStats.TryGetValue(d.DeckId, out deckStats))
					continue;
				foreach(var game in deckStats.Games.ToList())
				{
					deckStats.Games.Remove(game);
					d.DeckStats.Games.Add(game);
				}
				save = true;
			}
			if(save)
			{
				DeckStatsList.Save();
				Core.MainWindow.DeckPickerList.UpdateDecks();
			}
		}

		private static async void InitiateGameFilesCleanup()
		{
			while(!Core.MainWindow.IsLoaded || Core.MainWindow.WindowState == WindowState.Minimized || Core.MainWindow.FlyoutUpdateNotes.IsOpen)
				await Task.Delay(500);
			var result = await Core.MainWindow.ShowMessageAsync("Data maintenance required",
														  "Some files need to be cleaned up to help HDT run a bit better.\n\nThis shouldn't take too long, but you can also do it later.",
														  MessageDialogStyle.AffirmativeAndNegative,
														  new MetroDialogSettings() {AffirmativeButtonText = "start", NegativeButtonText = "ask again later"});
			if(result == MessageDialogResult.Negative)
				return;
			var controller = await Core.MainWindow.ShowProgressAsync("Cleaning up stuff...", "", true);
			await CleanUpGameFiles(controller);
			await controller.CloseAsync();
			if(controller.IsCanceled)
				await Core.MainWindow.ShowMessage("Cancelled", "No problem. You can just finish this later.");
			else
				await Core.MainWindow.ShowMessage("All done!", "");
		}

		private static string GamesDir => Path.Combine(Config.Instance.DataDir, "Games");
		private static async Task CleanUpGameFiles(ProgressDialogController controller)
		{
			var count = 0;
			int gamesCount;
			var lockMe = new object();
			var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
			await Task.Run(() =>
			{
				var games = DeckStatsList.Instance.DeckStats.Values.SelectMany(x => x.Games).Concat(DefaultDeckStats.Instance.DeckStats.SelectMany(x => x.Games)).ToList();
				gamesCount = games.Count;
				Parallel.ForEach(games, options, (game, loopState) =>
				{
					if(controller.IsCanceled)
					{
						loopState.Stop();
						return;
					}
					if(game.OpponentCards.Any())
						return;
					var oppCards = GetOpponentDeck(game);
					if(oppCards.Any())
						game.SetOpponentCards(oppCards);
					game.DeleteGameFile();
					lock(lockMe)
					{
						controller.SetProgress(1.0 * ++count / gamesCount);
					}
				});
			});
			DeckStatsList.Save();
			DefaultDeckStats.Save();
			if(!controller.IsCanceled)
			{
				try
				{
					Directory.Delete(GamesDir, true);
				}
				catch(Exception e)
				{
					Log.Error(e);
				}
			}
		}

		private static List<Card> GetOpponentDeck(GameStats gameStats)
		{
			var ignoreCards = new List<Card>();
			var cards = new List<Card>();
			var turnStats = gameStats.LoadTurnStats();
			ResolveSecrets(turnStats);
			foreach(var play in turnStats.SelectMany(turn => turn.Plays))
			{
				switch(play.Type)
				{
					case PlayType.OpponentPlay:
					case PlayType.OpponentDeckDiscard:
					case PlayType.OpponentHandDiscard:
					case PlayType.OpponentSecretTriggered:
						{
							var card = Database.GetCardFromId(play.CardId);
							if(Database.IsActualCard(card) && (card.PlayerClass == null || card.PlayerClass == gameStats.OpponentHero))
							{
								if(ignoreCards.Contains(card))
								{
									ignoreCards.Remove(card);
									continue;
								}
								var deckCard = cards.FirstOrDefault(c => c.Id == card.Id);
								if(deckCard != null)
									deckCard.Count++;
								else
									cards.Add(card);
							}
						}
						break;
					case PlayType.OpponentBackToHand:
						{
							var card = Database.GetCardFromId(play.CardId);
							if(Database.IsActualCard(card))
								ignoreCards.Add(card);
						}
						break;
				}
			}
			return cards.Where(x => x.Collectible).ToList();
		}

		private static void ResolveSecrets(IEnumerable<TurnStats> newTurnStats)
		{
			var unresolvedSecrets = 0;
			var triggeredSecrets = 0;
			TurnStats.Play candidateSecret = null;

			foreach(var play in newTurnStats.SelectMany(turn => turn.Plays))
			{
				// is secret play
				if((play.Type == PlayType.OpponentHandDiscard && play.CardId == "") || play.Type == PlayType.OpponentSecretPlayed)
				{
					unresolvedSecrets++;
					candidateSecret = play;
					play.Type = PlayType.OpponentSecretPlayed;
				}
				else if(play.Type == PlayType.OpponentSecretTriggered)
				{
					if(unresolvedSecrets == 1 && candidateSecret != null)
						candidateSecret.CardId = play.CardId;
					triggeredSecrets++;
					if(triggeredSecrets == unresolvedSecrets)
					{
						triggeredSecrets = 0;
						unresolvedSecrets = 0;
					}
				}
			}
		}
	}
}
