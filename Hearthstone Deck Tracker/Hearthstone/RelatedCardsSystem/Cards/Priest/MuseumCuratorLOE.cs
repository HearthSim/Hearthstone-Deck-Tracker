using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Battlecry: Discover a Deathrattle card. It costs (1) less."
public class MuseumCuratorLOE : ClassOrNeutralDeathrattleCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.MuseumCuratorLOE;
}

public class MuseumCuratorWONDERS : MuseumCuratorLOE
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.MuseumCuratorWONDERS;
}
