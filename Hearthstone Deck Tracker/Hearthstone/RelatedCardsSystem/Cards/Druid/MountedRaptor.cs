using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Deathrattle: Summon a random 1-Cost minion."
public class MountedRaptor : Cost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.MountedRaptor;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class MountedRaptorCore : MountedRaptor
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.MountedRaptorCorePlaceholder;
}
