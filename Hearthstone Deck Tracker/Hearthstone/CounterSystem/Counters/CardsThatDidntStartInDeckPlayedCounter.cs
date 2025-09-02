using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class CardsThatDidntStartInDeckPlayedCounter: NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.Techysaurus;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Mage.Techysaurus
	};

	public CardsThatDidntStartInDeckPlayedCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
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

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(!entity.Info.Created)
			return;

		if(DiscountIfCantPlay(tag, value, entity))
			return;

		if(tag != GameTag.ZONE)
			return;

		if(prevValue != (int)Zone.HAND)
			return;

		if((value is (int)Zone.PLAY or (int)Zone.SECRET) && gameState.CurrentBlock?.Type == "PLAY")
		{
			LastEntityToCount = entity;
			Counter++;
		}
	}
}
