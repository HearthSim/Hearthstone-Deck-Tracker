namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Transform ALL your other minions into ones that cost (3) more (keeping their original Costs)."
public class TheAzeriteMurloc : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Shaman.TheAzeriteMurlocToken;
	protected override int CostOffset => 3;
}
