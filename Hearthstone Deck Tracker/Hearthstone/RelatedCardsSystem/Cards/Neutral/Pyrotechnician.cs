using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "After you cast a spell, add a random Fire spell to your hand."
public class Pyrotechnician : FireSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Pyrotechnician;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
