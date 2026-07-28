using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Fill your hand with random Undead. They cost Health instead of Mana this turn."
public class ForgottenMillennium : UndeadMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.ForgottenMillennium;
	public override int Picks() => 1;
	public override int EventCount() => BoardFill.PlayerSlots;
	public override bool IsWithReplacement() => true;
}
