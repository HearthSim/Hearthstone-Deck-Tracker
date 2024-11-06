using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class LibramCostReductionCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_LibramCostReduction", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.AldorAttendant;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Paladin.LibramOfWisdom,
		HearthDb.CardIds.Collectible.Paladin.LibramOfClarity,
		HearthDb.CardIds.Collectible.Paladin.LibramOfDivinity,
		HearthDb.CardIds.Collectible.Paladin.LibramOfJustice,
		HearthDb.CardIds.Collectible.Paladin.LibramOfFaith,
		HearthDb.CardIds.Collectible.Paladin.LibramOfJudgment,
		HearthDb.CardIds.Collectible.Paladin.LibramOfHope,
	};

	public LibramCostReductionCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
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

	private static readonly Dictionary<string, int> EnchantLibramDict = new Dictionary<string, int>
	{
		{HearthDb.CardIds.NonCollectible.Neutral.AldorAttendant_AldorAttendantEnchantment, 1},
		{HearthDb.CardIds.NonCollectible.Neutral.AldorTruthseeker_AldorTruthseekerEnchantment, 2},
		{HearthDb.CardIds.NonCollectible.Paladin.InterstellarStarslicer_InterstellarLibramEnchantmentEnchantment, 1},
	};

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY)
			return;

		if(!EnchantLibramDict.ContainsKey(entity.Card.Id))
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter += EnchantLibramDict[entity.Card.Id];
	}
}
