using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class AvianaElunesChosenTurnCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.AvianaElunesChosen;

	public override string[] RelatedCards => new string[] {};
	public AvianaElunesChosenTurnCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public int AvianaEnchantmentsInPlay { get; set; }

	public override bool ShouldShow() => Game.IsTraditionalHearthstoneMatch && AvianaEnchantmentsInPlay > 0;

	public override string[] GetCardsToDisplay()
	{
		return new[]
		{
			HearthDb.CardIds.Collectible.Priest.AvianaElunesChosen
		};
	}

	public override string ValueToShow() => $"{Counter.ToString()} / 3";
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag == GameTag.ZONE && value == (int)Zone.PLAY &&
		   entity.Card.Id == HearthDb.CardIds.NonCollectible.Priest.AvianaElunesChosen_MoonCycleToken &&
		   entity.GetTag(GameTag.CONTROLLER) == (IsPlayerCounter ? Game.Player.Id : Game.Opponent.Id))
		{
			AvianaEnchantmentsInPlay++;
			OnCounterChanged();
		}

		// we only need the counter to work once because once the countdown is over the effect is permanent
		if(AvianaEnchantmentsInPlay >= 2)
			return;

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Priest.AvianaElunesChosen_MoonCycleToken)
			return;

		if(tag != GameTag.TAG_SCRIPT_DATA_NUM_1)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter = 3 - value;
	}
}
