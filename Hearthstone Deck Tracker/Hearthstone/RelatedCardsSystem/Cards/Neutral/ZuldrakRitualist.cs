namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Taunt. Battlecry: Summon three random 1-Cost minions for your opponent."
public class ZuldrakRitualist : GravelsnoutKnight
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ZuldrakRitualist;
	public override int EventCount() => 3;
}
