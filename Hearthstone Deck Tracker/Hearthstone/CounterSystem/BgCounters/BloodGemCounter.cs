using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class BloodGemCounter : StatsCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.BloodGem1;
	public override string[] RelatedCards => new string[] {};

	public BloodGemCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsBattlegroundsMatch && (AttackCounter > 3 || HealthCounter > 3);

	public override string[] GetCardsToDisplay()
	{
		return new []
		{
			HearthDb.CardIds.NonCollectible.Neutral.BloodGem1
		};
	}

	public override string ValueToShow() => $"+{Math.Max(1, AttackCounter)} / +{Math.Max(1, HealthCounter)}";
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsBattlegroundsMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) == IsPlayerCounter)
		{
			if(tag == GameTag.BACON_BLOODGEMBUFFATKVALUE)
			{
				AttackCounter = value + 1;
			}

			if(tag == GameTag.BACON_BLOODGEMBUFFHEALTHVALUE)
			{
				HealthCounter = value + 1;
			}
		}
	}
}
