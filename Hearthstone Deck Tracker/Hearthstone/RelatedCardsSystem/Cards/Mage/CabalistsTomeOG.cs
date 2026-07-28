using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Get 3 random Mage spells."
public class CabalistsTomeOG : MageSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.CabalistsTomeOG;
	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;
}

public class CabalistsTomeWONDERS : CabalistsTomeOG
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.CabalistsTomeWONDERS;
}

// "Battlecry: If you're holding a Dragon, Discover an upgraded Mage spell."
public class MalygosAspectOfMagic : MageSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.MalygosAspectOfMagic;
}
