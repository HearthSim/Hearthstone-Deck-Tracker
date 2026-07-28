using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Gigantify Battlecry: Discover an Undead. Reduce its Cost by this minion's Attack."
public class ToysnatchingGeist : ClassOrNeutralUndeadMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.ToysnatchingGeist;
}

public class ToysnatchingGeistToken : ToysnatchingGeist
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Deathknight.ToysnatchingGeist_ToysnatchingGeistToken;
}
