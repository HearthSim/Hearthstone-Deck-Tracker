using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Deal 2 damage. Shuffle a random Demon Hunter spell into your deck. Outcast: Do it again."
public class CosmicManifestations : DemonHunterSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.CosmicManifestations;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
