using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Taunt. Deathrattle: Summon a random 3-Cost Beast."
public class TeachersPet : Cost3BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.TeachersPet;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
