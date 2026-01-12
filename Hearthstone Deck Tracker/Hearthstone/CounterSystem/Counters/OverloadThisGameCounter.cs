using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class OverloadThisGameCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => "HearthDb.CardIds.Collectible.Shaman.HaywireHornswog";
	public override string[] RelatedCards => new string[]
	{
		"HearthDb.CardIds.Collectible.Shaman.HaywireHornswog"
	};

	public OverloadThisGameCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		return IsPlayerCounter && InPlayerDeckOrKnown(RelatedCards);
	}

	public override string[] GetCardsToDisplay()
	{
		return GetCardsInDeckOrKnown(RelatedCards).ToArray();
	}

	public override string ValueToShow() => Counter.ToString();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.OVERLOAD_THIS_GAME)
			return;

		if(value == 0)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if(controller == Game.Player.Id && IsPlayerCounter || controller == Game.Opponent.Id && !IsPlayerCounter)
		{
			Counter = value;
		}
	}
}
