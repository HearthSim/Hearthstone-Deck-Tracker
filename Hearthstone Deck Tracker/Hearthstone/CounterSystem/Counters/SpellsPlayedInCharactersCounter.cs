using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class SpellsPLayedInCharactersCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.SeaShanty;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Paladin.SeaShanty
	};

	public SpellsPLayedInCharactersCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 0 && OpponentMayHaveRelevantCards();
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

		var controller = entity.GetTag(GameTag.CONTROLLER);
		if(!((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter)))
			return;

		if(DiscountIfCantPlay(tag, value, entity))
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY && value != (int)Zone.SECRET)
			return;

		if(gameState.CurrentBlock?.Type != "PLAY")
			return;

		if(!entity.IsSpell)
			return;

		if(!entity.HasTag(GameTag.CARD_TARGET) ||
		   !Core.Game.Entities.TryGetValue(entity.GetTag(GameTag.CARD_TARGET), out var target))
			return;

		LastEntityToCount = entity;
		Counter++;
	}
}
