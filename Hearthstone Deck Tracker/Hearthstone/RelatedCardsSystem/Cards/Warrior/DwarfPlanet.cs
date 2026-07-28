using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Fill your board with random 2-Cost minions that attack random enemies."
public class DwarfPlanet : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.DwarfPlanet;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => BoardFill.PlayerSlots;
}
