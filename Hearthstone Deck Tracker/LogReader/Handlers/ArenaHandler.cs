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
			var deck = HearthMirror.Reflection.GetArenaDeck();
			if(deck?.Deck.Cards.Sum(x => x.Count) != 30)
				return;
			Log.Info($"Found new {deck.Deck.Hero} arena deck!");
			var recentArenaDecks = DeckList.Instance.Decks.Where(d => d.IsArenaDeck && d.Cards.Sum(x => x.Count) == 30).OrderByDescending(d => d.LastPlayedNewFirst).Take(15);
			if(recentArenaDecks.Any(d => d.Cards.All(c => deck.Deck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count))))
				Log.Info("...but we already have that one. Discarding.");
			else if(Core.Game.IgnoredArenaDecks.Contains(deck.Deck.Id))
				Log.Info("...but it was already discarded by the user. No automatic action taken.");
			else if(Config.Instance.SelectedArenaImportingBehaviour == ArenaImportingBehaviour.AutoAsk)
				Core.MainWindow.ShowNewArenaDeckMessageAsync(deck.Deck);
			else if(Config.Instance.SelectedArenaImportingBehaviour == ArenaImportingBehaviour.AutoImportSave)
			{
				Log.Info("...auto saving new arena deck.");
				Core.MainWindow.ImportArenaDeck(deck.Deck);
			}
		}
	}
}