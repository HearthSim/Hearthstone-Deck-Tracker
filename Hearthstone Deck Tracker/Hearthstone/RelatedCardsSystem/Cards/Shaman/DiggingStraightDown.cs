namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Deal 8 damage to a minion. Excavate a treasure."
public class DiggingStraightDown : ExcavateCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.DiggingStraightDown;
}
