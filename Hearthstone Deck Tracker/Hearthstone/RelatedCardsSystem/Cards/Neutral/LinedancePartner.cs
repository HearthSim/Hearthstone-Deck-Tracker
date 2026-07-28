using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If you're holding another 3-Cost card, summon a random 3-Cost minion."
public class LinedancePartner : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.LinedancePartner;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
