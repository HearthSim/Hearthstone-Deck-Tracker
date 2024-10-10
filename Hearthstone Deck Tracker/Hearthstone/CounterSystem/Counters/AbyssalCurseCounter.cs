using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class AbyssalCurseCounter : NumericCounter
{
	protected override string? CardIdToShowInUI =>
		HearthDb.CardIds.NonCollectible.Warlock.SirakessCultist_AbyssalCurseToken;

	public override string[] RelatedCards => new[]
	{
		HearthDb.CardIds.Collectible.Warlock.DraggedBelow,
		HearthDb.CardIds.Collectible.Warlock.SirakessCultist,
		HearthDb.CardIds.Collectible.Warlock.AbyssalWave,
		HearthDb.CardIds.Collectible.Warlock.Zaqul
	};

	public AbyssalCurseCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
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
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.Class);
	}

	public override string ValueToShow() => Counter.ToString();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Warlock.SirakessCultist_AbyssalCurseToken)
			return;

		if(tag != GameTag.TAG_SCRIPT_DATA_NUM_1)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter = value;
	}
}
