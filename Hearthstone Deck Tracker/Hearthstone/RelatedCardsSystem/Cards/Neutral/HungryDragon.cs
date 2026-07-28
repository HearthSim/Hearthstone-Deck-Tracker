namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Summon a random 1-Cost minion for your opponent."
public class HungryDragon : GravelsnoutKnight
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.HungryDragon;
}
