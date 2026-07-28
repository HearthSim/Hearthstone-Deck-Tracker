using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Summon a random 3-Cost minion."
public class FacelessSummoner : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.FacelessSummoner;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
