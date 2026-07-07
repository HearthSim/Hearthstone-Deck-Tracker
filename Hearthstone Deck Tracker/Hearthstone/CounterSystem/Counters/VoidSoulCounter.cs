using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class VoidSoulCounter: NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Demonhunter.VoidSoul;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Demonhunter.VoidSoul,
		HearthDb.CardIds.Collectible.Demonhunter.VoidBlast,
		HearthDb.CardIds.Collectible.Demonhunter.ViciousVoidscale,
		HearthDb.CardIds.Collectible.Demonhunter.StardustScythe,
	};

	public VoidSoulCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
		Counter = 1;
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 1;
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

		if(entity.CardId != HearthDb.CardIds.Collectible.Demonhunter.VoidSoul)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(DiscountIfCantPlay(tag, value, entity))
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value is not (int)Zone.PLAY || gameState.CurrentBlock?.Type != "PLAY") return;

		LastEntityToCount = entity;
		Counter++;
	}
}
