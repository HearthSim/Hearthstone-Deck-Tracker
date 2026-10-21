namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Old God Cost (1) less for each Deathrattle minion you've summoned this game."
// Shows the candidates of its Army of N'Zoth ability: "Resurrect 4 different Deathrattle minions."
// "Different" means each resurrected minion is distinct, so the grid is de-duplicated.
public class NzothDreamsEnd : ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.NzothDreamsEnd;

	protected override bool FilterCard(Card card) => card.TypeEnum == HearthDb.Enums.CardType.MINION && card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
