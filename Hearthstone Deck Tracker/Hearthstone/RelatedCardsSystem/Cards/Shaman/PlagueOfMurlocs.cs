
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform all minions into random Murlocs."
public class PlagueOfMurlocs : MurlocMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.PlagueOfMurlocs;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
