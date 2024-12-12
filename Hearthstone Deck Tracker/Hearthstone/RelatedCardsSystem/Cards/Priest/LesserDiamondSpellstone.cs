namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class LesserDiamondSpellstone: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.LesserDiamondSpellstone;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => true;
}
