using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards;

/// Shows the treasures the next Excavate can give: every reward of the player's current
/// Excavate tier, read off the ExcavateTierCounter, which owns the tier state.
public abstract class ExcavateCard : ICardWithRelatedCards
{
	public abstract string GetCardId();

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		var counters = player.Id == Core.Game.Player.Id
			? Core.Game.CounterManager.PlayerCounters
			: Core.Game.CounterManager.OpponentCounters;

		if(counters.FirstOrDefault(c => c is ExcavateTierCounter) is not ExcavateTierCounter excavateCounter)
			return new List<Card?>();

		return excavateCounter.GetCardsToDisplay().Select(Database.GetCardFromId).ToList();
	}
}
