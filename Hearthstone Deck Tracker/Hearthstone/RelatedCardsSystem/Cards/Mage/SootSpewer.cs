
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Spell Damage +1 Battlecry: If you control another Mech, get a random Fire spell."
public class SootSpewerGVG : FireSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.SootSpewerGVG;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class SootSpewerWONDERS : SootSpewerGVG
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.SootSpewerWONDERS;
}
