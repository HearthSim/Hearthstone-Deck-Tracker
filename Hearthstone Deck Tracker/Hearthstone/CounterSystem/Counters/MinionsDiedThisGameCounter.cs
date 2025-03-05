using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class MinionsDiedThisGameCounter: NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Deathknight.ReskaThePitBoss;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Deathknight.ReskaThePitBoss,
	};

	public MinionsDiedThisGameCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return !InPlayerDeckOrKnown(RelatedCards) && OpponentMayHaveRelevantCards();
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

		if(!entity.IsMinion) return;

		if(tag != GameTag.ZONE) return;

		if(prevValue != (int)Zone.PLAY) return;

		if(value != (int)Zone.GRAVEYARD) return;

		Counter++;
	}
}
