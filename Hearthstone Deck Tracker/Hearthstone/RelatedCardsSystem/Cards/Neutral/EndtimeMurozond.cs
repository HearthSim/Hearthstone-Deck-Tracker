using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Fill your board with random Dragons. Fully heal your hero. Skip your next turn."
public class EndtimeMurozond : DragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.EndtimeMurozond;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => BoardFill.PlayerSlots;
}
