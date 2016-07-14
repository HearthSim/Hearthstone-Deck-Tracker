#region

using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using static Hearthstone_Deck_Tracker.LogReader.HsLogReaderConstants;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class ArenaHandler
	{
		public void Handle(LogLineItem logLine, IHsGameState gameState, IGame game)
		{
			if(!logLine.Line.Contains("SetDraftMode - ACTIVE_DRAFT_DECK") || (DateTime.Now - logLine.Time).TotalSeconds > 5)
				return;
			DeckManager.AutoImportArena(Config.Instance.SelectedArenaImportingBehaviour ?? ArenaImportingBehaviour.AutoImportSave);
		}
	}
}