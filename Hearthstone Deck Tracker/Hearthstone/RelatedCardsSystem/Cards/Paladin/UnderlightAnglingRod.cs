namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "After your Hero attacks, add a random Murloc to your hand."
public class UnderlightAnglingRod : MurlocKnight
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.UnderlightAnglingRod;
}

public class UnderlightAnglingRodCorePlaceholder : UnderlightAnglingRod
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.UnderlightAnglingRodCorePlaceholder;
}
