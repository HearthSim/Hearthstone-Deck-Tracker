using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter.AnimalCompanionGenerator;

public abstract class AnimalCompanionGenerator
{
	public List<Card?> GetRelatedCards(Player player)
	{
		if(player.Id != Core.Game.Player.Id)
			return new List<Card?>();

		var animalCompanionCounter = Core.Game.CounterManager.PlayerCounters.FirstOrDefault(c => c is AnimalCompanionCounter);
		if(animalCompanionCounter is AnimalCompanionCounter acCounter)
			return acCounter.Companions.Select(c => new Card(c)).ToList()!;

		return new List<Card?>();
	}
}
