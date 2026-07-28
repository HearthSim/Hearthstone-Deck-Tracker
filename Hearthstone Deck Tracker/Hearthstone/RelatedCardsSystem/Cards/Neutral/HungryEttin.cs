namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Taunt Battlecry: Summon a random 2-Cost minion for your opponent."
public class HungryEttin : PilotedShredder
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.HungryEttin;
}
