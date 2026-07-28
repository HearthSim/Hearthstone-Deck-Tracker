using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Both players get a random spell. Yours costs (2) less."
public class SpellslingerTGT : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.SpellslingerTGT;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}

public class SpellslingerWONDERS : SpellslingerTGT
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.SpellslingerWONDERS;
}
