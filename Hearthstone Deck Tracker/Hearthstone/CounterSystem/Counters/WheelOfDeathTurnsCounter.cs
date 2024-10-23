using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class WheelOfDeathTurnsCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.WheelOfDeath;

	public override string[] RelatedCards => new string[] {};
	public WheelOfDeathTurnsCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public bool IsWheelOfDeathInPlay { get; set; }

	public override bool ShouldShow() => Game.IsTraditionalHearthstoneMatch && IsWheelOfDeathInPlay;

	public override string[] GetCardsToDisplay()
	{
		return new[]
		{
			HearthDb.CardIds.Collectible.Warlock.WheelOfDeath
		};
	}

	public override string ValueToShow() => Counter.ToString();
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag == GameTag.ZONE &&
		   value == (int)Zone.PLAY &&
		   entity.Card.Id == HearthDb.CardIds.Collectible.Warlock.WheelOfDeath &&
		   entity.GetTag(GameTag.CONTROLLER) == (IsPlayerCounter ? Game.Player.Id : Game.Opponent.Id))
		{
			IsWheelOfDeathInPlay = true;
		}

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Warlock.WheelofDEATH_WheelOfDeathCounterEnchantment)
			return;

		if(tag != GameTag.TAG_SCRIPT_DATA_NUM_1)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter = value;
	}
}
