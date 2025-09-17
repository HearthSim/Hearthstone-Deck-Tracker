namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class InventorBoom: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.InventorBoom;

	public new bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentGameType, Core.Game.CurrentFormatType , opponent.OriginalClass) && GetRelatedCards(opponent).Count > 0;
	}

	protected override bool FilterCard(Card card) => card.IsMech() && card.Cost >= 5;

	protected override bool ResurrectsMultipleCards() => true;
}
