using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class SilverHandRecruitBuffCounter : StatsCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_SilverHandRecruitBuff", useCardLanguage: true);

	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Paladin.SilverHandRecruitLegacyToken1;
	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Paladin.BrashBattlemaster,
		HearthDb.CardIds.Collectible.Paladin.ResilientSavior,
		HearthDb.CardIds.Collectible.Paladin.EmboldeningBlade,
	};

	public SilverHandRecruitBuffCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsTraditionalHearthstoneMatch && (AttackCounter > 0 || HealthCounter > 0);

	public override string[] GetCardsToDisplay()
	{
		return  IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);
	}

	public override string ValueToShow() => $"+{Math.Max(0, AttackCounter)} / +{Math.Max(0, HealthCounter)}";
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY)
			return;

		if(entity.Card.Id == HearthDb.CardIds.NonCollectible.Paladin.EmboldeningBlade_EmboldenedEnchantment1)
		{
			AttackCounter += 1;
			HealthCounter += 1;
			return;
		}

		if(entity.Card.Id == HearthDb.CardIds.NonCollectible.Paladin.BrashBattlemaster_RecruitsMightEnchantment)
		{
			AttackCounter += 1;
			return;
		}

		if(entity.Card.Id == HearthDb.CardIds.NonCollectible.Paladin.ResilientSavior_RecruitsResilienceEnchantment)
		{
			HealthCounter += 1;
			return;
		}
	}
}
