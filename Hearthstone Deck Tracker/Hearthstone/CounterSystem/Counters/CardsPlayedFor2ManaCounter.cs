using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class CardsPlayedFor2ManaCounter: NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.JadeGuardians;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Rogue.JadeGuardians,
		HearthDb.CardIds.Collectible.Rogue.LotusTroublemaker
	};

	public CardsPlayedFor2ManaCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 2 && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return RelatedCards;
	}

	public override string ValueToShow() => Counter.ToString();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(DiscountIfCantPlay(tag, value, entity))
			return;

		if(tag != GameTag.NUM_RESOURCES_SPENT_THIS_GAME)
			return;

		if(gameState.CurrentBlock?.Type != "PLAY")
			return;

		if(value - prevValue != 2)
			return;

		var playedCard = new Card(gameState.CurrentBlock.CardId ?? "");

		if(playedCard.TypeEnum == CardType.HERO_POWER)
			return;

		LastEntityToCount = entity;
		Counter++;
	}
}
