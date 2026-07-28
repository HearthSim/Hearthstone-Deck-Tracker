using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a spell or pick a mystery choice."
public class VulperaScoundrel : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.VulperaScoundrel;
}

public class VulperaScoundrelCorePlaceholder : VulperaScoundrel
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.VulperaScoundrelCorePlaceholder;
}

// "Add a random spell to your hand."
public class MysteryChoiceToken : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.VulperaScoundrel_MysteryChoiceToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
