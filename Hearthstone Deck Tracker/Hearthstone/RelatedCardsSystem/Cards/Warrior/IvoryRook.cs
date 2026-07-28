using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Battlecry: Discover a Taunt minion. Gain Armor equal to its Cost."
public class IvoryRook : FrightenedFlunky
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.IvoryRook;
}
