using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Deal $3 damage to a character. If it dies, summon a random Demon."
public class BaneOfDoomExpert1 : CallOfTheVoidLegacy
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.BaneOfDoomExpert1;
}

public class BaneOfDoomVanilla : BaneOfDoomExpert1
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.BaneOfDoomVanilla;
}

public class BaneOfDoomWONDERS : BaneOfDoomExpert1
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.BaneOfDoomWONDERS;
}
