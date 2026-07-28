using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Whenever you cast a spell, add a random Priest spell to your hand."
public class LyraTheSunshard : PriestSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.LyraTheSunshard;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class LyraTheSunshardCorePlaceholder : LyraTheSunshard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.LyraTheSunshardCorePlaceholder;
}
