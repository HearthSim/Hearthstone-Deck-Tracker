using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Battlecry: Fill your hand with random Dragons. If you spent 25 Mana while holding this, they cost (1)."
public class MerithraOfTheDream : DragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.MerithraOfTheDream;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
