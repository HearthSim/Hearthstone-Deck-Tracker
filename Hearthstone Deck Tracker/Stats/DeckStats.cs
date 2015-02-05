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
		//public string HearthStatsDeckVersionId;
		public string Name;

		public DeckStats()
		{
			Games = new List<GameStats>();
		}

		public DeckStats(Deck deck)
		{
			Name = deck.Name;
			Games = new List<GameStats>();
			//HearthStatsDeckVersionId = deck.HearthStatsDeckVersionId;
			HearthStatsDeckId = deck.HearthStatsId;
			DeckId = deck.DeckId;
		}

		[XmlIgnore]
		public bool HasHearthStatsDeckId
		{
			get { return !string.IsNullOrEmpty(HearthStatsDeckId); }
		}

		//[XmlIgnore]
		//public bool HasHearthStatsDeckVersionId
		//{
		//	get { return !string.IsNullOrEmpty(HearthStatsDeckVersionId); }
		//}

		public void AddGameResult(GameResult result, string opponentHero, string playerHero)
		{
			Games.Add(new GameStats(result, opponentHero, playerHero));
		}

		public void AddGameResult(GameStats gameStats)
		{
			Games.Add(gameStats);
		}

		public bool BelongsToDeck(Deck deck)
		{
			if(HasHearthStatsDeckId && deck.HasHearthStatsId)
				return HearthStatsDeckId.Equals(deck.HearthStatsIdForUploading);
			//if(HasHearthStatsDeckId && deck.HasHearthStatsId && HasHearthStatsDeckVersionId && deck.HasHearthStatsDeckVersionId)
			//	return HearthStatsDeckId.Equals(deck.HearthStatsId) && HearthStatsDeckVersionId.Equals(deck.HearthStatsDeckVersionId);
			return DeckId == deck.DeckId;
		}
	}
}