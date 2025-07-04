using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class DiveTheGolakkaDepthsBuffCounter : StatsCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.DiveTheGolakkaDepths;
	public override string[] RelatedCards => new string[] {};

	public DiveTheGolakkaDepthsBuffCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsTraditionalHearthstoneMatch && (AttackCounter > 0 || HealthCounter > 0);

	public override string[] GetCardsToDisplay()
	{
		return new []
		{
			HearthDb.CardIds.Collectible.Paladin.DiveTheGolakkaDepths
		};
	}

	public override string ValueToShow() => $"+{Math.Max(0, AttackCounter)} / +{Math.Max(0, HealthCounter)}";
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.Card.Id != HearthDb.CardIds.Collectible.Paladin.DiveTheGolakkaDepths)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(tag != GameTag.TAG_SCRIPT_DATA_NUM_1)
			return;

		AttackCounter = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
		HealthCounter = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
	}
}
