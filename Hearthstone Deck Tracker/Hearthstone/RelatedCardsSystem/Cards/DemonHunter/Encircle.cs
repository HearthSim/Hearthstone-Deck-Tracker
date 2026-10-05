using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Summon five 3/1 Shredclaws with Rush. Summon a random 1-Cost minion for your opponent."
public class Encircle : Cost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.Encircle;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
