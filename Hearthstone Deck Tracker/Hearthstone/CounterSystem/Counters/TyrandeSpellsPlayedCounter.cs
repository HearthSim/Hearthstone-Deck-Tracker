using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class TyrandeSpellsPlayedCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.Tyrande;

	public override string[] RelatedCards => new string[] {};
	public TyrandeSpellsPlayedCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public int TyrandeEnchantmentsInPlay { get; set; }

	public override bool ShouldShow() => Game.IsTraditionalHearthstoneMatch && TyrandeEnchantmentsInPlay > 0;

	public override string[] GetCardsToDisplay()
	{
		return new[]
		{
			HearthDb.CardIds.Collectible.Priest.Tyrande
		};
	}

	public override string ValueToShow() => $"{Counter.ToString()} / 3";
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag == GameTag.ZONE &&
		   entity.Card.Id == HearthDb.CardIds.NonCollectible.Priest.Tyrande_PullOfTheMoonEnchantment &&
		   entity.GetTag(GameTag.CONTROLLER) == (IsPlayerCounter ? Game.Player.Id : Game.Opponent.Id))
		{
			if(value == (int)Zone.PLAY)
			{
				TyrandeEnchantmentsInPlay++;
				Counter = 0;
				OnCounterChanged();
			}
			else if(value is (int)Zone.GRAVEYARD)
			{
				TyrandeEnchantmentsInPlay--;
				OnCounterChanged();
			}
		}


		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Priest.Tyrande_PullOfTheMoonEnchantment)
			return;

		if(tag != GameTag.TAG_SCRIPT_DATA_NUM_1)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter = value;
	}
}
