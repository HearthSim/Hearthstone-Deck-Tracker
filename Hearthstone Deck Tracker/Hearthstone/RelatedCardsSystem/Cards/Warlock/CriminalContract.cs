using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Summon three random 3-Cost minions."
public class CriminalContract : Cost3MinionPool
{
	// TODO: replace with HearthDb constant once CAP ships
	public override string GetCardId() =>
		"HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_CriminalContractToken";

	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;
}
