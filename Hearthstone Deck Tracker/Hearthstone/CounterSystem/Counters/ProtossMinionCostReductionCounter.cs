using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class ProtossMinionCostReductionCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_ProtossMinionCostReduction", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Invalid.PhotonCannon;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Priest.Sentry,
		HearthDb.CardIds.Collectible.Invalid.PhotonCannon,
		HearthDb.CardIds.Collectible.Invalid.Artanis,
	};

	public ProtossMinionCostReductionCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return Counter > 0 || InPlayerDeckOrKnown(RelatedCards);
		return Counter > 0 && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);
	}

	public override string ValueToShow() => Counter.ToString();

	private static readonly int _artanisDbfId =
		HearthDb.Cards.All.TryGetValue(
			HearthDb.CardIds.Collectible.Invalid.Artanis, out var artanis) ? artanis.DbfId : -1;

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY)
			return;

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Neutral.ConstructPylons_PsionicPowerEnchantment)
			return;

		// artanis discounts by 2
		var amount = entity.GetTag(GameTag.CREATOR_DBID) == _artanisDbfId ? 2 : 1;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter += amount;
	}
}
