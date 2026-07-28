using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Choose One - Give your hero +2 Attack this turn; or get a random Druid card."
public class SecretIngredient : DruidCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.SecretIngredient;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
