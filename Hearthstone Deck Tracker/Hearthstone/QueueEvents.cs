#region

using System;
using System.Collections.Generic;
using System.Linq;
using HearthMirror;
using HearthMirror.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HearthWatcher.EventArgs;
using static Hearthstone_Deck_Tracker.Enums.Hearthstone.Mode;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class QueueEvents
	{
		private readonly List<Mode> LettuceModes = new()
		{
			LETTUCE_BOUNTY_BOARD,
			LETTUCE_MAP,
			LETTUCE_PLAY,
			LETTUCE_COOP,
			LETTUCE_FRIENDLY,
			LETTUCE_BOUNTY_TEAM_SELECT,
		};

		private readonly List<Mode> Modes = new()
		{
			TAVERN_BRAWL,
			TOURNAMENT,
			DRAFT,
			FRIENDLY,
			ADVENTURE,
			BACON
		};

		private bool _isInQueue = false;
		public bool IsInQueue => _isInQueue;

		public QueueEvents(IGame game)
		{
			_game = game;
		}

		private readonly IGame _game;

		public void Handle(QueueEventArgs e)
		{
			_isInQueue = e.IsInQueue;

			if(!_game.IsInMenu)
				return;
			if(!Modes.Contains(_game.CurrentMode) && !LettuceModes.Contains(_game.CurrentMode))
				return;

			if(_game.CurrentMode == Mode.TOURNAMENT)
				Core.Overlay.SetConstructedQueue(e.IsInQueue);

			if(_game.CurrentMode == Mode.BACON)
				Core.Overlay.SetBaconQueue(e.IsInQueue);

			if(e.IsInQueue)
			{
				_game.MetaData.EnqueueTime = DateTime.Now;

				Log.Info($"Now in queue");
				if(_game.CurrentMode == DRAFT)
					_game.CurrentSelectedDeck = DeckImporter.ArenaInfoCache?.Deck;
				else
				{
					var selectedId = GetSelectedDeckId(_game.CurrentMode);
					_game.CurrentSelectedDeck = selectedId > 0 ? Reflection.Client.GetDecks()?.FirstOrDefault(deck => deck.Id == selectedId) : null;
				}
				if(!Config.Instance.AutoDeckDetection)
					return;
				if(_game.CurrentMode is TOURNAMENT or FRIENDLY or ADVENTURE or TAVERN_BRAWL)
				{
					var deckId = GetSelectedDeckId(_game.CurrentMode) ?? 0;
					if(deckId > 0)
					{
						DeckManager.AutoSelectDeckById(_game, deckId);
					}
					else if(deckId < 0)
					{
						DeckManager.AutoSelectTemplateDeckByDeckTemplateId(_game, (int)-deckId);
					}
				}
				else if(_game.CurrentMode == DRAFT)
					AutoSelectArenaDeck();
				else if(_game.CurrentMode == BACON)
				{
					if(DeckList.Instance.ActiveDeck != null)
					{
						Log.Info("Switching to no-deck mode for battlegrounds");
						Core.MainWindow.SelectDeck(null, true);
					}
				}
				else if(LettuceModes.Contains(_game.CurrentMode))
				{
					if(DeckList.Instance.ActiveDeck != null)
					{
						Log.Info("Switching to no-deck mode for mercenaries");
						Core.MainWindow.SelectDeck(null, true);
					}
				}
			}
			else
			{
				Log.Info($"No longer in queue");
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

		private static long? GetSelectedDeckId(Mode mode)
		{
			var deckPickerState = Reflection.Client.GetDeckPickerState();
			if(deckPickerState?.SelectedDeck is long selectedDeckId)
				return selectedDeckId;
			if(deckPickerState?.SelectedTemplateDeck is int selectedTemplateDeck)
				return -selectedTemplateDeck;
			if(mode == TAVERN_BRAWL)
				return Reflection.Client.GetEditedDeck()?.Id;
			return null;
		}
	}
}
