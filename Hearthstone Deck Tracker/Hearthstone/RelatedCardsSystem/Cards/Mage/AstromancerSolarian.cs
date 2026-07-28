using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Spell Damage +1 Battlecry: Cast 5 random Mage spells (targets enemies if possible)."
public class SolarianPrimeToken : MageSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Mage.AstromancerSolarian_SolarianPrimeToken;
	public override int Picks() => 1;
	public override int EventCount() => 5;
	public override bool IsWithReplacement() => true;
}
