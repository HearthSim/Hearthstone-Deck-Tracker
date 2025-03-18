namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class SuccumbToMadness: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.SuccumbToMadness;

	public new bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.OriginalClass) && GetRelatedCards(opponent).Count > 0;
	}

	protected override bool FilterCard(Card card) => card.IsDragon();

	protected override bool ResurrectsMultipleCards() => false;
}
