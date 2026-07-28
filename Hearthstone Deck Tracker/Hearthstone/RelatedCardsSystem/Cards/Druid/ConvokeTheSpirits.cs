using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Cast 8 random Druid spells (targets chosen randomly)."
public class ConvokeTheSpirits : DruidSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.ConvokeTheSpirits;
	public override int Picks() => 1;
	public override int EventCount() => 8;
	public override bool IsWithReplacement() => true;
}

public class ConvokeTheSpiritsCore : ConvokeTheSpirits
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.ConvokeTheSpiritsCorePlaceholder;
}
