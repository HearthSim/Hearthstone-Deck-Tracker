using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class CardsDrawnThisTurnCounter: NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_CardsDrawnThisTurn", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.EverythingMustGo;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Rogue.EverythingMustGo,
		HearthDb.CardIds.Collectible.Demonhunter.IreboundBrute,
		HearthDb.CardIds.Collectible.Demonhunter.LionsFrenzy,
		HearthDb.CardIds.Collectible.Demonhunter.Momentum,
		HearthDb.CardIds.Collectible.Demonhunter.ArguniteGolem,
		HearthDb.CardIds.Collectible.Demonhunter.Mindbender,
	};

	public CardsDrawnThisTurnCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		return IsPlayerCounter && InPlayerDeckOrKnown(RelatedCards);
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

		if(tag == GameTag.NUM_TURNS_IN_PLAY)
		{
			Counter = 0;
			return;
		}

		if(tag != GameTag.ZONE)
			return;

		if(prevValue != (int)Zone.DECK)
			return;

		if(value != (int)Zone.HAND)
			return;

		Counter++;

	}
}
