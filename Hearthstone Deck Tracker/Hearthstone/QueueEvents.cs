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

		public QueueEvents(IGame game)
		{
			_game = game;
		}

		private readonly IGame _game;

		public void Handle(QueueEventArgs e)
		{
			if(!_game.IsInMenu)
				return;
			if(!Modes.Contains(_game.CurrentMode) && !LettuceModes.Contains(_game.CurrentMode))
				return;

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
				if(new[] { TOURNAMENT, FRIENDLY, ADVENTURE, TAVERN_BRAWL }.Contains(_game.CurrentMode))
					DeckManager.AutoSelectDeckById(_game, GetSelectedDeckId(_game.CurrentMode));
				else if(_game.CurrentMode == DRAFT)
					AutoSelectArenaDeck();
				else if(_game.CurrentMode == BACON)
				{
					if(DeckList.Instance.ActiveDeck != null)
					{
						Log.Info("Switching to no-deck mode for battlegrounds");
						Core.MainWindow.SelectDeck(null, true);
					}
					Core.Overlay.ShowTier7PreLobby(false, false);
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
				if(_game.CurrentMode == BACON && e.Previous != FindGameState.SERVER_GAME_CONNECTING)
					Core.Overlay.ShowTier7PreLobby(true, false, 0);
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
			var selectedDeckId = Reflection.Client.GetSelectedDeckInMenu();
			if(selectedDeckId > 0)
				return selectedDeckId;
			if(mode != TAVERN_BRAWL)
				return 0;
			return Reflection.Client.GetEditedDeck()?.Id ?? 0;
		}
	}
}
