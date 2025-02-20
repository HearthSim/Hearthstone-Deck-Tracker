namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class ChiaDrake : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Druid.ChiaDrake;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}

public class ChiaDrakeMini : ChiaDrake
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.ChiaDrake_ChiaDrakeToken;
}
