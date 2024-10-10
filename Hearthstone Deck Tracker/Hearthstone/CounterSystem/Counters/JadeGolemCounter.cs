using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class JadeGolemCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.JadeGolem1;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Druid.JadeIdol,
		HearthDb.CardIds.Collectible.Druid.JadeBlossom,
		HearthDb.CardIds.Collectible.Druid.JadeBehemothGANGS,
		HearthDb.CardIds.Collectible.Druid.JadeBehemothWONDERS,
		HearthDb.CardIds.Collectible.Rogue.JadeSwarmerGANGS,
		HearthDb.CardIds.Collectible.Rogue.JadeSwarmerWONDERS,
		HearthDb.CardIds.Collectible.Rogue.JadeTelegram,
		HearthDb.CardIds.Collectible.Rogue.JadeShurikenGANGS,
		HearthDb.CardIds.Collectible.Rogue.JadeShurikenWONDERS,
		HearthDb.CardIds.Collectible.Shaman.JadeClaws,
		HearthDb.CardIds.Collectible.Shaman.JadeLightningGANGS,
		HearthDb.CardIds.Collectible.Shaman.JadeLightningWONDERS,
		HearthDb.CardIds.Collectible.Shaman.JadeChieftainGANGS,
		HearthDb.CardIds.Collectible.Shaman.JadeChieftainWONDERS,
		HearthDb.CardIds.Collectible.Neutral.JadeSpiritGANGS,
		HearthDb.CardIds.Collectible.Neutral.JadeSpiritWONDERS,
		HearthDb.CardIds.Collectible.Neutral.AyaBlackpawGANGS,
		HearthDb.CardIds.Collectible.Neutral.AyaBlackpawWONDERS,
	};

	public JadeGolemCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(IsPlayerCounter)
			return Counter > 0 || InPlayerDeckOrKnown(RelatedCards);
		return Counter > 0 && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return new[]
		{
			HearthDb.CardIds.NonCollectible.Neutral.JadeGolem1
		};
	}

	public override string ValueToShow()
	{
		var jadeSize = Math.Min(Counter + 1, 30);

		return $"{jadeSize}/{jadeSize}";
	}

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.JADE_GOLEM)
			return;

		if(value == 0)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if(controller == Game.Player.Id && IsPlayerCounter || controller == Game.Opponent.Id && !IsPlayerCounter)
			Counter = value;
	}
}
