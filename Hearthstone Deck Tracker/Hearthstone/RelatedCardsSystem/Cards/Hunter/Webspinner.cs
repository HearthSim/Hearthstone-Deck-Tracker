using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Deathrattle: Get a random Beast."
public class Webspinner : BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Webspinner;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class WebspinnerCorePlaceholder : Webspinner
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.WebspinnerCorePlaceholder;
}
