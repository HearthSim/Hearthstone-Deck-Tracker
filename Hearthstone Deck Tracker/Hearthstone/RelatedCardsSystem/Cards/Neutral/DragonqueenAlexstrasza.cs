namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If your deck has no duplicates, add 2 other random Dragons to your hand. They cost (0)."
public class DragonqueenAlexstrasza : BoneDrake
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.DragonqueenAlexstrasza;
	public override int EventCount() => 2;
}
