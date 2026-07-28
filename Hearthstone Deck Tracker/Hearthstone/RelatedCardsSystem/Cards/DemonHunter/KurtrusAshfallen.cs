using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Add a Demon Hunter spell to your hand."
// Demon Hunter spell pool inherited from CosmicManifestations (explicit class name in the text).
public class KurtrusAshfallenSTORMWIND : DemonHunterSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.KurtrusAshfallenSTORMWIND;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

// "Rush. After this attacks and kills a minion, add a Fel spell to your hand."
public class KurtrusAshfallenToken1 : FelSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.KurtrusAshfallenToken1;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
