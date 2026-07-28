
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Inspire: Add a random spell to your hand."
public class NexusChampionSaraad : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.NexusChampionSaraad;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
