using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "After you cast a spell on a minion, add a Priest spell to your hand."
public class SethekkVeilweaver : PriestSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.SethekkVeilweaver;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
