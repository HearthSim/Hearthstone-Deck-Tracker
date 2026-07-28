using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Summon a random undead."
public class GraveCall : UndeadMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Deathknight.GraveCall;
	public override int Picks() => 1;
	public override int EventCount() => BoardFill.PlayerSlots;
	public override bool IsWithReplacement() => true;
}
