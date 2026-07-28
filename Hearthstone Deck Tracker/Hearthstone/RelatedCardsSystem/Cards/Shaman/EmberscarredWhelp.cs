using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Discover a 5-Cost card. Gain 1 Mana Crystal next turn only."
public class EmberscarredWhelp : ClassOrNeutralCost5CardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.EmberscarredWhelp;
}
