using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class TheCeaselessExpanseCounter: NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.TheCeaselessExpanse;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Neutral.TheCeaselessExpanse,
	};

	public TheCeaselessExpanseCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return !InPlayerDeckOrKnown(RelatedCards) && Counter >= 80 && OpponentMayHaveRelevantCards();
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

		if(!Game.IsMulliganDone)
			return;

		if(tag != GameTag.ZONE) return;

		switch (prevValue)
		{
			// card was drawn
			case (int)Zone.DECK when value == (int)Zone.HAND:
				Counter++;
				return;
			// card was played
			case (int)Zone.HAND when value == (int)Zone.PLAY:
				Counter++;
				return;
			case (int)Zone.HAND when value == (int)Zone.SECRET:
				Counter++;
				return;
			//card was destroyed
			case (int)Zone.PLAY when value == (int)Zone.GRAVEYARD && (entity.IsMinion || entity.IsWeapon || entity.IsLocation):
				Counter++;
				return;
		}

	}
}
