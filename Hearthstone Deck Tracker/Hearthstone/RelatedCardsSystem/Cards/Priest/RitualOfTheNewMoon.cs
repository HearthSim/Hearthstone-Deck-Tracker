using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Summon two random 3-Cost minions. (Cast 3 spells to summon 6-Cost minions instead.)"
// The upgraded state is a separate token below.
public class RitualOfTheNewMoon : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.RitualOfTheNewMoon;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}

// "Summon two random 6-Cost minions."
public class RitualOfTheFullMoonToken : Cost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Priest.RitualoftheNewMoon_RitualOfTheFullMoonToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
