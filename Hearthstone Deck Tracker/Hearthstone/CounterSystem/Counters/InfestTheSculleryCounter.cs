using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class InfestTheSculleryCounter : NumericCounter
{
	private const int BaseCost = 4;

	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Druid.InfestTheScullery;

	public override string[] RelatedCards => new[]
	{
		HearthDb.CardIds.Collectible.Druid.InfestTheScullery
	};

	public InfestTheSculleryCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 0 && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return RelatedCards;
	}

	public override string ValueToShow() => Math.Min(BaseCost + Counter, 10).ToString();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.ATTACKING || value == 0)
			return;

		if(!entity.IsHero)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		Counter++;
	}
}
