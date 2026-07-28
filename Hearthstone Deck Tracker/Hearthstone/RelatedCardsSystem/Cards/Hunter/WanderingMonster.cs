using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Secret: When an enemy attacks your hero, summon a 3-Cost minion as the new target."
public class WanderingMonster : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.WanderingMonster;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class WanderingMonsterCorePlaceholder : WanderingMonster
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.WanderingMonsterCorePlaceholder;
}
