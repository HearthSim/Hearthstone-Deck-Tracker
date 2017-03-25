#region

using System;
using System.Linq;
using HearthMirror;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HearthWatcher.LogReader;
using static Hearthstone_Deck_Tracker.Enums.Hearthstone.Mode;
using Deck = HearthMirror.Objects.Deck;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class FullScreenFxHandler
	{
		private DateTime _lastQueueTime;
		public void Handle(LogLine logLine, IGame game)
		{
			var match = LogConstants.BeginBlurRegex.Match(logLine.Line);
			if(match.Success && game.IsInMenu && new[] {TAVERN_BRAWL, TOURNAMENT, DRAFT, FRIENDLY, ADVENTURE}.Contains(game.CurrentMode))
			{
				game.MetaData.EnqueueTime = logLine.Time;
				Log.Info($"Now in queue ({logLine.Time})");
				if((DateTime.Now - logLine.Time).TotalSeconds > 5 || !game.IsInMenu || logLine.Time <= _lastQueueTime)
					return;
				_lastQueueTime = logLine.Time;
				if(game.CurrentMode == DRAFT)
					game.CurrentSelectedDeck = DeckImporter.ArenaInfoCache?.Deck;
				else
				{
					var selectedId = GetSelectedDeckId(game.CurrentMode);
					game.CurrentSelectedDeck = selectedId > 0 ? Reflection.GetDecks().FirstOrDefault(deck => deck.Id == selectedId) : null;
				}
				if(!Config.Instance.AutoDeckDetection)
					return;
				if(new[] {TOURNAMENT, FRIENDLY, ADVENTURE, TAVERN_BRAWL}.Contains(game.CurrentMode))
					AutoSelectDeckById(game.CurrentMode);
				else if(game.CurrentMode == DRAFT)
					AutoSelectArenaDeck();
			}
		}

		private void AutoSelectArenaDeck()
		{
			var hsDeck = DeckImporter.ArenaInfoCache?.Deck;
			if(hsDeck == null)
				return;
			var selectedDeck = DeckList.Instance.Decks.FirstOrDefault(x => x.HsId == hsDeck.Id);
			if(selectedDeck == null)
			{
				Log.Warn($"No arena deck with id={hsDeck.Id} found");
				return;
			}
			Log.Info($"Switching to arena deck deck: {selectedDeck.Name}");
			Core.MainWindow.SelectDeck(selectedDeck, true);
		}

		private static long GetSelectedDeckId(Mode mode)
		{
			var selectedDeckId = Reflection.GetSelectedDeckInMenu();
			if(selectedDeckId > 0)
				return selectedDeckId;
			if(mode != TAVERN_BRAWL)
				return 0;
			return Reflection.GetEditedDeck()?.Id ?? 0;
		}

		private static void AutoSelectDeckById(Mode mode)
		{
			var selectedDeckId = GetSelectedDeckId(mode);
			if(selectedDeckId <= 0)
			{
				Log.Info("No selected deck found, using no-deck mode");
				Core.MainWindow.SelectDeck(null, true);
				return;
			}
			DeckManager.AutoImportConstructed(false, mode == TAVERN_BRAWL);
			var selectedDeck = DeckList.Instance.Decks.FirstOrDefault(x => x.HsId == selectedDeckId);
			if(selectedDeck == null)
			{
				Log.Warn($"No deck with id={selectedDeckId} found");
				return;
			}
			Log.Info("Found selected deck: " + selectedDeck.Name);
			var hsDeck = DeckImporter.ConstructedDecksCache.FirstOrDefault(x => x.Id == selectedDeckId);
			if(hsDeck != null && !selectedDeck.Cards.All(c => hsDeck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count)))
			{
				var version = selectedDeck.Versions.FirstOrDefault(v => v.Cards.All(c => hsDeck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count)));
				if(version != null)
				{
					selectedDeck.SelectVersion(version);
					Log.Info("Switching to version: " + version.Version.ShortVersionString);
				}
			}
			else if(Equals(selectedDeck, DeckList.Instance.ActiveDeck))
			{
				Log.Info("Already using the correct deck");
				return;
			}
			Core.MainWindow.SelectDeck(selectedDeck, true);
		}
	}
}
