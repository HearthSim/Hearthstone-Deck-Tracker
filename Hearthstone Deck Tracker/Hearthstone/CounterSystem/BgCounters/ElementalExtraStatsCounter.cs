using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class ElementalsExtraStatsCounter : StatsCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.SandSwirler;
	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.NonCollectible.Neutral.SandSwirler,
		HearthDb.CardIds.NonCollectible.Neutral.GlowingCinder,
	};

	public ElementalsExtraStatsCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override string LocalizedName => LocUtil.Get("Counter_ElementalExtraStats", useCardLanguage: true);

	public override bool ShouldShow() => Game.IsBattlegroundsMatch && (AttackCounter > 0 || HealthCounter > 0);

	public override string[] GetCardsToDisplay() => RelatedCards;

	public override string ValueToShow() => $"+{Math.Max(0, AttackCounter)} / +{Math.Max(0, HealthCounter)}";

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsBattlegroundsMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(tag == GameTag.BACON_ELEMENTAL_BUFFATKVALUE)
			AttackCounter = value;

		if(tag == GameTag.BACON_ELEMENTAL_BUFFHEALTHVALUE)
			HealthCounter = value;
	}
}
