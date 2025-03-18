using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class YsondreCounter: NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Warrior.Ysondre;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Warrior.Ysondre,
	};

	public YsondreCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public bool OpponentHadYsondreInPlay { get; set; }

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return (Counter > 0 && OpponentMayHaveRelevantCards()) || OpponentHadYsondreInPlay;
	}

	public override string[] GetCardsToDisplay()
	{
		return RelatedCards;
	}

	public override string ValueToShow() => (Counter + 1).ToString();
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(!Game.IsMulliganDone)
			return;

		if(entity.CardId != HearthDb.CardIds.Collectible.Warrior.Ysondre) return;

		if(tag != GameTag.ZONE) return;

		if(value == (int)Zone.PLAY)
		{
			if(entity.GetTag(GameTag.CONTROLLER) == Game.Opponent.Id && !IsPlayerCounter)
				OpponentHadYsondreInPlay = true;
			return;
		}

		if(prevValue != (int)Zone.PLAY) return;

		if(value != (int)Zone.GRAVEYARD) return;

		var controller = entity.GetTag(GameTag.CONTROLLER);
		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter++;
	}
}
