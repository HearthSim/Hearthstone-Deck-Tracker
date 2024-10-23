using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class ChaoticTendrilCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.ChaoticTendril;
	public override string[] RelatedCards => new[]
	{
		HearthDb.CardIds.Collectible.Neutral.ChaoticTendril
	};

	public ChaoticTendrilCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
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
		return RelatedCards;
	}

	public override string ValueToShow() => Math.Min(Counter + 1, 10).ToString();
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.ZONE || gameState.CurrentBlock?.Type != "PLAY")
			return;

		var isCurrentController = IsPlayerCounter ? entity.IsControlledBy(Game.Player.Id)
			: entity.IsControlledBy(Game.Opponent.Id);

		if(!isCurrentController)
			return;

		if(entity.Card.Id != HearthDb.CardIds.Collectible.Neutral.ChaoticTendril)
			return;

		Counter++;
	}
}
