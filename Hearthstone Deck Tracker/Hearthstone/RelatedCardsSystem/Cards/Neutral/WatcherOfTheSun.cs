using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Get a random Holy spell. Forge: Also restore 6 Health to your hero."
public class WatcherOfTheSun : HolySpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WatcherOfTheSun;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

// "Forged Battlecry: Get a random Holy spell. Restore 6 Health to your hero."
public class WatcherOfTheSunToken : WatcherOfTheSun
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.WatcheroftheSun_WatcherOfTheSunToken;
}
