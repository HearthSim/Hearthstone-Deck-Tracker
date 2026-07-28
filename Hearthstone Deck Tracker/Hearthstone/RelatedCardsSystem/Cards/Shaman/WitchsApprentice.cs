using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Taunt Battlecry: Add a random Shaman spell to your hand."
public class WitchsApprentice : ShamanSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.WitchsApprentice;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class WitchsApprenticeCore : WitchsApprentice
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.WitchsApprenticeCore;
}
