using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Battlecry: Discover a Fel spell."
public class Scorchreaver : ClassOrNeutralFelSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.Scorchreaver;
}
