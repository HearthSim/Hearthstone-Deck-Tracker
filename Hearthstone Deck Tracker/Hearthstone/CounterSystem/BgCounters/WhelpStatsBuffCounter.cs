using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class WhelpStatsBuffCounter : StatsCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.TwilightHatchling_TwilightWhelpToken;
	public override string LocalizedName => LocUtil.Get("Counter_WhelpBuff", useCardLanguage: true);

	public override string[] RelatedCards => new []
	{
		HearthDb.CardIds.NonCollectible.Neutral.BurgeoningWhelp,
		HearthDb.CardIds.NonCollectible.Neutral.TwilightHatchling_TwilightWhelpToken,
		HearthDb.CardIds.NonCollectible.Neutral.BlueWhelp
	};

	public WhelpStatsBuffCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsBattlegroundsMatch && (AttackCounter > 0 || HealthCounter > 0);

	public override string[] GetCardsToDisplay() => RelatedCards;

	public override string ValueToShow() => $"+{AttackCounter} / +{HealthCounter}";

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsBattlegroundsMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Neutral.BurgeoningWhelp_WhelpBuffPlayerEnchantDnt)
			return;

		if(tag == GameTag.TAG_SCRIPT_DATA_NUM_1)
			AttackCounter = value;

		if(tag == GameTag.TAG_SCRIPT_DATA_NUM_2)
			HealthCounter = value;
	}
}
