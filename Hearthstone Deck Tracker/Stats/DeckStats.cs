using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Stats;

public class DeckStats
{
	public Guid DeckId;

	[XmlArray(ElementName = "Games")]
	[XmlArrayItem(ElementName = "Game")]
	public List<GameStats> Games;

	public string? Name;

	public DeckStats()
	{
		Games = new List<GameStats>();
	}

	public DeckStats(Deck deck)
	{
		Name = deck.Name;
		Games = new List<GameStats>();
		DeckId = deck.DeckId;
	}
}
