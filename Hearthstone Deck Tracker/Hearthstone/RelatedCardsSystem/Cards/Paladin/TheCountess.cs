using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a Legendary minion from another class. It costs (0)."
public class LegendaryInvitationToken : OffClassLegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Paladin.TheCountess_LegendaryInvitationToken;
}
