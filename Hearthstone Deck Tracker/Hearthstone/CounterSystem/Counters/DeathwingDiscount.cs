using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class DeathwingDiscount : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_DeathwingDiscount", useCardLanguage: true);

	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Invalid.DeathwingWorldbreakerHeroic;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Invalid.Ultraxion,
		HearthDb.CardIds.Collectible.Invalid.DeathwingWorldbreakerHeroic,
	};

	public DeathwingDiscount(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		return Counter != 0;
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);
	}

	public override string ValueToShow() => Counter.ToString();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.CardId != HearthDb.CardIds.NonCollectible.Neutral.Ultraxion_UltraxionHeraldedEnchantment)
			return;

		if(tag != GameTag.TAG_SCRIPT_DATA_NUM_1)
			return;

		if(value == 0)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if(controller == Game.Player.Id && IsPlayerCounter || controller == Game.Opponent.Id && !IsPlayerCounter)
		{
			Counter = -value;
		}
	}
}
