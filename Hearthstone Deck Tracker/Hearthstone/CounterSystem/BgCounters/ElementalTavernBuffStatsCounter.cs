using System;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class ShopBuffStatsCounter : StatsCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.NomiKitchenNightmare;
	public override string[] RelatedCards => new string[]
	{
		// HearthDb.CardIds.NonCollectible.Neutral.NomiKitchenNightmare_NomiPlayerEnchantDnt,
		// HearthDb.CardIds.NonCollectible.Neutral.DazzlingLightspawn_DazzlingLightspawnPlayerEnchantDnt,
		// HearthDb.CardIds.NonCollectible.Neutral.DancingBarnstormer_DancingBarnstormerPlayerEnchantDnt,
		// HearthDb.CardIds.NonCollectible.Neutral.LivingAzerite_LivingAzeritePlayerEnchantDntEnchantment,
		// HearthDb.CardIds.NonCollectible.Neutral.NomiSticker_NomiStickerPlayerEnchantDnt,
		// HearthDb.CardIds.NonCollectible.Neutral.DuneDweller_DuneDwellerPlayerEnchantDnt,
		// HearthDb.CardIds.NonCollectible.Neutral.BlazingGreasefire_BlazingGreasefirePlayerEnchantDnt,
		// HearthDb.CardIds.NonCollectible.Neutral.AligntheElements_AlignTheElementsPlayerEnchDnt,
		HearthDb.CardIds.NonCollectible.Neutral.ElementalShopBuffPlayerEnchantmentDnt,
	};

	public ShopBuffStatsCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override string LocalizedName => LocUtil.Get("Counter_ElementalTavernBuff", useCardLanguage: true);

	public override bool ShouldShow() => Game.IsBattlegroundsMatch && (AttackCounter > 1 || HealthCounter > 1);

	public override string[] GetCardsToDisplay()
	{
		return new []
		{
			HearthDb.CardIds.NonCollectible.Neutral.NomiKitchenNightmare,
			HearthDb.CardIds.NonCollectible.Neutral.DazzlingLightspawn,
			HearthDb.CardIds.NonCollectible.Neutral.DancingBarnstormer,
			HearthDb.CardIds.NonCollectible.Neutral.LivingAzerite,
			HearthDb.CardIds.NonCollectible.Neutral.DuneDweller,
			HearthDb.CardIds.NonCollectible.Neutral.BlazingGreasefire,
			HearthDb.CardIds.NonCollectible.Neutral.AlignTheElements,
		};
	}

	public override string ValueToShow() => $"+{Math.Max(1, AttackCounter)} / +{Math.Max(1, HealthCounter)}";
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsBattlegroundsMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(tag == GameTag.ZONE
		   && (value == (int)Zone.PLAY || (value == (int)Zone.SETASIDE && prevValue == (int)Zone.PLAY))
		   && RelatedCards.Contains(entity.CardId))
		{
			OnCounterChanged();
		}

		if(!RelatedCards.Contains(entity.CardId))
			return;

		var buffValue = value - prevValue;

		if(tag == GameTag.TAG_SCRIPT_DATA_NUM_1 && entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.NomiSticker_NomiStickerPlayerEnchantDnt)
		{
			AttackCounter += buffValue;
			HealthCounter += buffValue;

		}
		else
		{
			if(tag == GameTag.TAG_SCRIPT_DATA_NUM_1)
			{
				AttackCounter += buffValue;
			}

			if(tag == GameTag.TAG_SCRIPT_DATA_NUM_2)
			{
				HealthCounter += buffValue;
			}
		}
	}
}
