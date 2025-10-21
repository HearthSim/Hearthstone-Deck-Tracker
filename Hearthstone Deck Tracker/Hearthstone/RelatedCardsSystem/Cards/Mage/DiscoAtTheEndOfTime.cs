﻿using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class DiscoAtTheEndOfTime : ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.DiscoAtTheEndOfTime;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.GetTag(GameTag.SECRET) > 0 &&
		       (Helper.WildOnlySets.Contains(card.Set) ||
		        Helper.ClassicOnlySets.Contains(card.Set));
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.All(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
