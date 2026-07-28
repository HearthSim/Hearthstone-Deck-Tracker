namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Choose One - Get 2 random Nature spells; or Reduce the Cost of spells in your hand by (1)."
public class LifebindersGift : DruidOfRegrowth
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.LifebindersGift;
}
