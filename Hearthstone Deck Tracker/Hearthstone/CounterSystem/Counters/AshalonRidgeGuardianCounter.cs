using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class AshalonRidgeGuardianCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Shaman.SpiritoftheMountain_AshalonRidgeGuardianToken;

	public override string[] RelatedCards => new string[] {};
	public AshalonRidgeGuardianCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	private readonly List<string> _adapts = new();

	public override bool ShouldShow() => Game.IsTraditionalHearthstoneMatch && _adapts.Count > 0;

	public override string ValueToShow() => "";

	public override string[] GetCardsToDisplay() => _adapts.ToArray();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Shaman.SpiritoftheMountain_PerfectEvolutionToken)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
		{
			if(tag is GameTag.TAG_SCRIPT_DATA_NUM_1 or GameTag.TAG_SCRIPT_DATA_NUM_2)
			{
				var card = Database.GetCardFromDbfId(value, false);
				if(card == null)
					return;
				_adapts.Add(card.Id);
				OnCounterChanged();
			}
		}

	}

}
