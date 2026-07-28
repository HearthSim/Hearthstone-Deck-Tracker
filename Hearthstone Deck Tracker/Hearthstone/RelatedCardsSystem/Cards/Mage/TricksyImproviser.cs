using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Prepare Battlecry: If you've cast a spell this turn, cast a random Mage Secret."
public class TricksyImproviser : MageSecretPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.TricksyImproviser;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
