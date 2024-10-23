using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class PogoHopperCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.PogoHopper;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Rogue.PogoHopper
	};

	public PogoHopperCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
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

	public override string ValueToShow() {
		var pogoSize = Counter * 2 + 1;

		return $"{pogoSize}/{pogoSize}";
	}

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

		if(entity.Card.Id != HearthDb.CardIds.Collectible.Rogue.PogoHopper)
			return;

		Counter++;
	}
}
