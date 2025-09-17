using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class CardsDrawnThisGameCounter: NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.PlayhouseGiant;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Neutral.PlayhouseGiant,
	};

	public CardsDrawnThisGameCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		var playhouseGiant = Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.PlayhouseGiant);
		if(playhouseGiant == null) return false;
		return Game.Opponent.OriginalClass?.ToUpperInvariant() == "ROGUE" &&
			playhouseGiant.IsCardLegal(Game.CurrentGameType, Game.CurrentFormatType) &&
			Counter >= 10;
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

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(tag != GameTag.ZONE)
			return;

		if(prevValue != (int)Zone.DECK)
			return;

		if(value != (int)Zone.HAND)
			return;

		Counter++;

	}
}
