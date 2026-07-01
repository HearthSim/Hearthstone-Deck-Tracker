using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class AnimalCompanionCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Hunter.AnimalCompanionCore;
	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Hunter.AnimalCompanionCore,
		HearthDb.CardIds.Collectible.Hunter.AnimalCompanionLegacy,
		HearthDb.CardIds.Collectible.Hunter.AnimalCompanionVanilla,
		HearthDb.CardIds.Collectible.Hunter.BrollBearmantle,
		HearthDb.CardIds.Collectible.Hunter.CallOfTheWild,
		HearthDb.CardIds.Collectible.Hunter.CallOfTheWildCore,
		HearthDb.CardIds.Collectible.Hunter.OpenTheCages,
		HearthDb.CardIds.Collectible.Hunter.PatchworkPals,
		HearthDb.CardIds.Collectible.Hunter.RoamFree,
		HearthDb.CardIds.Collectible.Hunter.Spiritspeaker,
		HearthDb.CardIds.Collectible.Hunter.ToMySide,

	};

	public string[] Companions = new[]
	{
		HearthDb.CardIds.NonCollectible.Hunter.HufferLegacy,
		HearthDb.CardIds.NonCollectible.Hunter.LeokkLegacy,
		HearthDb.CardIds.NonCollectible.Hunter.MishaLegacy,
	};

	private HashSet<string> _opponentKnownCompanions = new HashSet<string>();

	public AnimalCompanionCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		return Counter > 3;
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ? Companions : _opponentKnownCompanions.ToArray();
	}

	public override string ValueToShow() => string.Format(LocUtil.Get("Counter_AnimalCompanionCost"), Counter.ToString());

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);
		var isPlayerController = controller == Game.Player.Id;
		if(isPlayerController != IsPlayerCounter)
			return;

		if(entity.CardId is HearthDb.CardIds.Collectible.Hunter.TamePet
		   or HearthDb.CardIds.Collectible.Hunter.MigratingElekk
		   or HearthDb.CardIds.Collectible.Hunter.RoamFree)
		{
			if(tag != GameTag.HIDDEN_SCRIPT_DATA_4 && tag != GameTag.HIDDEN_SCRIPT_DATA_5 && tag != GameTag.HIDDEN_SCRIPT_DATA_6)
				return;

			if(!HearthDb.Cards.AllByDbfId.TryGetValue(value, out var card))
				return;

			if(!IsPlayerCounter)
			{
				_opponentKnownCompanions.Clear();
			}

			switch(tag)
			{
				case GameTag.HIDDEN_SCRIPT_DATA_4:
					Companions[0] = card.Id;
					OnCounterChanged();
					break;
				case GameTag.HIDDEN_SCRIPT_DATA_5:
					Companions[1] = card.Id;
					OnCounterChanged();
					break;
				case GameTag.HIDDEN_SCRIPT_DATA_6:
					Companions[2] = card.Id;
					OnCounterChanged();
					break;
			}
			Counter = card.Cost;
		}

		if(IsPlayerCounter)
			return;

		if(HandleOpponentSummon(tag, gameState, entity, value, prevValue))
		{
			OnCounterChanged();
		}
	}

	private bool HandleOpponentSummon(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(tag != GameTag.ZONE)
			return false;

		if(value != (int)Zone.PLAY)
			return false;

		if(!RelatedCards.Contains(gameState.CurrentBlock?.CardId))
			return false;

		if(!entity.Card.IsBeast())
			return false;

		if(entity.Card.Cost != Counter)
			return false;

		_opponentKnownCompanions.Add(entity.Card.Id);

		return true;
	}
}
