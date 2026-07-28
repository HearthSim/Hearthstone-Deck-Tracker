using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Fill your board with random Undead."
public class TheScourge : UndeadMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.TheScourge;
	public override int Picks() => 1;
	public override int EventCount() => BoardFill.PlayerSlots;
	public override bool IsWithReplacement() => true;
}

public class TheScourgeCorePlaceholder : TheScourge
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.TheScourgeCorePlaceholder;
}
