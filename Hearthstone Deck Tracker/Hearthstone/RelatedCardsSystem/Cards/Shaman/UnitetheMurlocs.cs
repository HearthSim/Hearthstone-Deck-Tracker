using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Fill your hand with random Murlocs."
public class MegafinToken : MurlocMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Shaman.UnitetheMurlocs_MegafinToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
