using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Discover a Naga."
public class AzsharasHatcheryToken : ClassOrNeutralNagaMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.OrgozoatheTender_AzsharasHatcheryToken;
}

// "Discover 2 Naga."
public class AzsharasHatchery : AzsharasHatcheryToken
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.OrgozoatheTender_AzsharasHatchery;
	public override int EventCount() => 2;
}
