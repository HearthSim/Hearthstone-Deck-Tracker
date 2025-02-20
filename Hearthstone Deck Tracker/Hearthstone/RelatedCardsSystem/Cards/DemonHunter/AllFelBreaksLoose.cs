namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class AllFelBreaksLoose: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.AllFelBreaksLoose;

	protected override bool FilterCard(Card card) => card.IsDemon();

	protected override bool ResurrectsMultipleCards() => false;
}

public class AllFelBreaksLooseInfused: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.AllFelBreaksLoose_AllFelBreaksLooseToken;

	protected override bool FilterCard(Card card) => card.IsDemon();

	protected override bool ResurrectsMultipleCards() => true;
}
