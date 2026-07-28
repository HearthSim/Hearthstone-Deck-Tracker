using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Summon four random Murlocs, then shoot them at enemy minions. (You pick the targets!)"
public class Grunty : MurlocMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Grunty;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 4;
}
