using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Discover a Taunt minion. Give it +1/+2."
public class IKnowAGuyGANGS : FrightenedFlunky
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.IKnowAGuyGANGS;
}

public class IKnowAGuyCore : IKnowAGuyGANGS
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.IKnowAGuyCore;
}

public class IKnowAGuyWONDERS : IKnowAGuyGANGS
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.IKnowAGuyWONDERS;
}
