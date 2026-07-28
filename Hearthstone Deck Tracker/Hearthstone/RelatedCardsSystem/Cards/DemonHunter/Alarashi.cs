using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Battlecry: Transform minions in your hand into random Demons. (They keep their original stats and Cost.)"
public class Alarashi : DemonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.Alarashi;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
