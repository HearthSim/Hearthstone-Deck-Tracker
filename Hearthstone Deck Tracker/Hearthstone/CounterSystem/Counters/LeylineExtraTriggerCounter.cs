using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class LeylineExtraTriggerCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.SurgeNeedle;
	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Mage.SurgeNeedle,
		HearthDb.CardIds.Collectible.Mage.TheArcanomicon,
	};

	public LeylineExtraTriggerCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		return Counter > 0;
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

		if((int)tag != 4283)
			return;

		if(value == 0)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);
		var isPlayerController = controller == Game.Player.Id;
		if(isPlayerController != IsPlayerCounter)
			return;

		Counter = value;
	}
}
