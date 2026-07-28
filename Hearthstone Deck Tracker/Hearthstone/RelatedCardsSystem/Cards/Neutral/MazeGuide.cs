namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Summon a random 2-Cost minion."
public class MazeGuide : PilotedShredder
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.MazeGuide;
}

public class MazeGuideCore : MazeGuide
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.MazeGuideCore;
}
