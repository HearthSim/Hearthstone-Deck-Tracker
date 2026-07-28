using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Add a random Deathrattle minion to your hand."
public class ShallowGravedigger : DeathrattleMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ShallowGravedigger;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class ShallowGravediggerCorePlaceholder : ShallowGravedigger
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ShallowGravediggerCorePlaceholder;
}
