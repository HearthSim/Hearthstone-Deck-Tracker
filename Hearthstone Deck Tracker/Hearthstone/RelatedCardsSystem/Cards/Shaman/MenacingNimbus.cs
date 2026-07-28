using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Add a random Elemental to your hand."
public class MenacingNimbus : ElementalMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MenacingNimbus;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class MenacingNimbusCorePlaceholder : MenacingNimbus
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MenacingNimbusCorePlaceholder;
}
