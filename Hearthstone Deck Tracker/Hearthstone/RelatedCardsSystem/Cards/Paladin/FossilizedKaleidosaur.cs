namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Battlecry: Gain two random Bonus Effects. Excavate a treasure."
public class FossilizedKaleidosaur : ExcavateCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.FossilizedKaleidosaur;
}
