using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Cast a random spell that costs (5) or MORE (targets chosen randomly)."
public class ServantOfYoggSaronOG : CostAtLeast5SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ServantOfYoggSaronOG;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class ServantOfYoggSaronWONDERS : ServantOfYoggSaronOG
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ServantOfYoggSaronWONDERS;
}
