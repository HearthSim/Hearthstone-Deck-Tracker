
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Summon two random 4-Cost minions. Costs (1) less for each card you've drawn this turn."
public class EverythingMustGo : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.EverythingMustGo;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
