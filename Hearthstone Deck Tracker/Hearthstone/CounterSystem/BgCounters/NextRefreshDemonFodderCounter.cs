using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class DemonFodderCounter : NumericCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.LaboratoryAssistant_DemonFodderToken1;
	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.NonCollectible.Neutral.LaboratoryAssistant_DemonFodderToken1,
		HearthDb.CardIds.NonCollectible.Neutral.LaboratoryAssistant,
		HearthDb.CardIds.NonCollectible.Neutral.WoodlandDefiler,
		HearthDb.CardIds.NonCollectible.Neutral.TwistedWrathguard

	};

	public DemonFodderCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override string LocalizedName => LocUtil.Get("Counter_NextRefreshDemonFodder", useCardLanguage: true);

	public override bool ShouldShow() => Game.IsBattlegroundsMatch && Counter > 0;

	public override string[] GetCardsToDisplay() => RelatedCards;

	public override string ValueToShow() => Counter.ToString();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsBattlegroundsMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(entity.GetTag(GameTag.ZONE) == (int)Zone.SETASIDE)
			return;

		if(tag == GameTag.BACON_FODDERS_IN_REFRESH)
		{
			Counter = value;
			OnCounterChanged();
		}
	}
}
