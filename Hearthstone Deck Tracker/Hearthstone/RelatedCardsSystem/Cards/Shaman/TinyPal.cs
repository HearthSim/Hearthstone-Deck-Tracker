using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// After your hero attacks, summon a random 3-Cost minion. Give it Taunt.
public class TinyPal3 : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Shaman.TinyPal_TinyPalToken3;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

// After your hero attacks, get a random Battlecry minion. It costs (2) less.
public class TinyPal4 : ClassOrNeutralBattlecryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Shaman.TinyPal_TinyPalToken4;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
