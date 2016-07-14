#region

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class DeckStats
	{
		public Guid DeckId;

		[XmlArray(ElementName = "Games")]
		[XmlArrayItem(ElementName = "Game")]
		public List<GameStats> Games;

		public string HearthStatsDeckId;
		public string Name;

		public DeckStats()
		{
			Games = new List<GameStats>();
		}

		public DeckStats(Deck deck)
		{
			Name = deck.Name;
			Games = new List<GameStats>();
			HearthStatsDeckId = deck.HearthStatsId;
			DeckId = deck.DeckId;
		}

		[XmlIgnore]
		public bool HasHearthStatsDeckId => !string.IsNullOrEmpty(HearthStatsDeckId);

		public void AddGameResult(GameResult result, string opponentHero, string playerHero) => Games.Add(new GameStats(result, opponentHero, playerHero));

		public void AddGameResult(GameStats gameStats) => Games.Add(gameStats);

		public bool BelongsToDeck(Deck deck) => HasHearthStatsDeckId && deck.HasHearthStatsId
													? HearthStatsDeckId.Equals(deck.HearthStatsIdForUploading) : DeckId == deck.DeckId;
	}
}