using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Inspire: Summon a random Murloc."
public class MurlocKnight : MurlocMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.MurlocKnight;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
