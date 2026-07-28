using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "At the start of your turn, add a random Demon Hunter spell to your hand."
// Demon Hunter spell pool inherited from CosmicManifestations (explicit class name in the text).
public class IllidanStormrageTHEBARRENS1 : DemonHunterSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.IllidanStormrageTHE_BARRENS1;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
