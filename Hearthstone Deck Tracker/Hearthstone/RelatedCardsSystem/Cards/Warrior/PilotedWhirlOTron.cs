using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Deathrattle: Summon a random 3-Cost minion."
public class PilotedWhirlOTronToken : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Warrior.PilotedWhirlOTronToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
