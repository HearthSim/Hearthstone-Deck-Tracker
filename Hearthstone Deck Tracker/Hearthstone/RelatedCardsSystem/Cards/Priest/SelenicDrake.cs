using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Elusive At the end of your turn, get a random Dragon."
public class SelenicDrake : DragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.SelenicDrake;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
