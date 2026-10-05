namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Resurrect 4 different Deathrattle minions." — N'Zoth, Dream's End's ability.
// "Different" means each resurrected minion is distinct, so the grid is de-duplicated.
public class ArmyOfNzoth : ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.NZothDreamsEnd_ArmyOfNzothToken;

	protected override bool FilterCard(Card card) => card.TypeEnum == HearthDb.Enums.CardType.MINION && card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
