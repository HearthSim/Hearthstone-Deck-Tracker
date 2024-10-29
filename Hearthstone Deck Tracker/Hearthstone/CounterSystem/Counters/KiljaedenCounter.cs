using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class KiljaedenCounter : StatsCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.Kiljaeden;
	public override string[] RelatedCards => new string[] {};

	public KiljaedenCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsTraditionalHearthstoneMatch && (AttackCounter > 0 || HealthCounter > 0);

	public override string[] GetCardsToDisplay()
	{
		return new []
		{
			HearthDb.CardIds.Collectible.Neutral.Kiljaeden
		};
	}

	public override string ValueToShow() => $"+{Math.Max(0, AttackCounter)} / +{Math.Max(0, HealthCounter)}";
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Neutral.Kiljaeden_KiljaedensPortalEnchantment)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		AttackCounter = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
		HealthCounter = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
	}
}
