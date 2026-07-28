using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Discover a Taunt minion. If you play it this turn, repeat this."
public class TrainingSession : FrightenedFlunky
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.TrainingSession;
}
