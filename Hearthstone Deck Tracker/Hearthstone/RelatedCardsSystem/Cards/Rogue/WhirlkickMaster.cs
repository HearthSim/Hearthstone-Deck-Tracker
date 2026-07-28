using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Whenever you play a Combo card, add a random Combo card to your hand."
public class WhirlkickMaster : ComboCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.WhirlkickMaster;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
