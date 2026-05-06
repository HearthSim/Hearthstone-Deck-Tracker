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
		HearthDb.CardIds.Collectible.Hunter.TalyaEarthstrider,
		HearthDb.CardIds.Collectible.Hunter.TamePet,
		HearthDb.CardIds.Collectible.Hunter.RoamFree,
		HearthDb.CardIds.Collectible.Hunter.MigratingElekk,
		HearthDb.CardIds.Collectible.Hunter.AnimalCompanionCore,
		HearthDb.CardIds.Collectible.Hunter.AnimalCompanionLegacy,
		HearthDb.CardIds.Collectible.Hunter.AnimalCompanionVanilla,
	};

	public string[] Companions = new[]
	{
		HearthDb.CardIds.NonCollectible.Hunter.HufferLegacy,
		HearthDb.CardIds.NonCollectible.Hunter.LeokkLegacy,
		HearthDb.CardIds.NonCollectible.Hunter.MishaLegacy,
	};

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
		return IsPlayerCounter ? Companions : new string[] {};
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
			if(tag != GameTag.TAG_SCRIPT_DATA_NUM_4 && tag != GameTag.TAG_SCRIPT_DATA_NUM_5 && tag != GameTag.TAG_SCRIPT_DATA_NUM_6)
				return;

			if(!HearthDb.Cards.AllByDbfId.TryGetValue(value, out var card))
				return;

			switch(tag)
			{
				case GameTag.TAG_SCRIPT_DATA_NUM_4:
					Companions[0] = card.Id;
					break;
				case GameTag.TAG_SCRIPT_DATA_NUM_5:
					Companions[1] = card.Id;
					break;
				case GameTag.TAG_SCRIPT_DATA_NUM_6:
					Companions[2] = card.Id;
					break;
			}
			Counter = card.Cost;
		}
	}
}
