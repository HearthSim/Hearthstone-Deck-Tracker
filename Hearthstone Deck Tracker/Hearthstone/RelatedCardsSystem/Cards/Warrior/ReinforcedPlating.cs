namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Gain 6 Armor. Excavate a treasure."
public class ReinforcedPlating : ExcavateCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.ReinforcedPlating;
}
