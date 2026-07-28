using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If you're holding a Dragon, summon 2 random Murlocs."
public class Skyfin : MurlocMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Skyfin;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
