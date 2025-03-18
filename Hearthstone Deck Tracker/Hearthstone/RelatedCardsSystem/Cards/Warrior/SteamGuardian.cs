using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class SteamGuardian : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.SteamGuardian;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE, card.Type == "Spell");
}
