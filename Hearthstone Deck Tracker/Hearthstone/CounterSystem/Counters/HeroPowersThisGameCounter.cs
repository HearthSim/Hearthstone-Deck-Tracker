using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class HeroPowersThisGameCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Shaman.GlowrootLure;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Shaman.GlowrootLure,
	};

	public HeroPowersThisGameCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 1 && OpponentMayHaveRelevantCards();
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

		if(tag != GameTag.NUM_TIMES_HERO_POWER_USED_THIS_GAME)
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
