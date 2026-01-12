using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class MinionsDiedThisTurnCounter: NumericCounter
{
	protected override string? CardIdToShowInUI => "HearthDb.CardIds.Collectible.Demonhunter.RemnantofRage";

	public override string[] RelatedCards => new string[]
	{
		"HearthDb.CardIds.Collectible.Demonhunter.RemnantofRage",
	};

	public MinionsDiedThisTurnCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
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

		if(!Game.IsMulliganDone)
			return;

		if(tag == GameTag.NUM_TURNS_IN_PLAY)
		{
			Counter = 0;
			return;
		}

		if(!entity.IsMinion) return;

		if(tag != GameTag.ZONE) return;

		if(prevValue != (int)Zone.PLAY) return;

		if(value != (int)Zone.GRAVEYARD) return;

		Counter++;
	}
}
