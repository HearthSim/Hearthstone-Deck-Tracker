using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Casts When Drawn. Summon a random Dragon."
public class DreamPortalToken : DragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.YseraUnleashed_DreamPortalToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}

// "Battlecry: Shuffle 7 Dream Portals into your deck. When drawn, summon a random Dragon."
public class YseraUnleashed : DragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.YseraUnleashed;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 7;
}
