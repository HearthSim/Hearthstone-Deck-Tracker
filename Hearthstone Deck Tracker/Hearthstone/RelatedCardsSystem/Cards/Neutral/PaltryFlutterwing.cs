namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Summon a random 2-Cost minion that is Dormant for 2 turns."
public class PaltryFlutterwing : PilotedShredder
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PaltryFlutterwing;
}
