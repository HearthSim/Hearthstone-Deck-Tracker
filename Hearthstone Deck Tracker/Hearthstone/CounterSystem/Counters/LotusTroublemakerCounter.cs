using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class LotusTroublemakerCounter: NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.LotusTroublemaker;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Rogue.LotusTroublemaker,
	};

	public LotusTroublemakerCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public LotusTroublemakerCounter(int id, bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
		EntityId = id;
		Counter = 1;
	}

	public int? EntityId;

	private bool _leftTheHand = false;

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(!IsPlayerCounter) return false;
		if(EntityId == null) return false;
		if(_leftTheHand) return false;

		return Game.Player.Hand.Any(x => x.Id == EntityId && x.CardId == CardIdToShowInUI);
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

		if(!entity.IsControlledBy(Game.Player.Id))
			return;

		if(tag == GameTag.ZONE && entity.Id == EntityId && value != (int)Zone.HAND)
		{
			_leftTheHand = true;
			return;
		}

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
