using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Each time you play a minion this turn, add a random Shaman spell to your hand."
public class Melomania : ShamanSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Melomania;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
