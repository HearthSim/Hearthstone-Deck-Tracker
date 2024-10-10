using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class CtunCounter : StatsCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.CthunOG;

	public override string[] RelatedCards => new[]
	{
		HearthDb.CardIds.Collectible.Neutral.CthunOG
	};

	public CtunCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return (AttackCounter > 2 || HealthCounter > 2) && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return RelatedCards;
	}

	public override string ValueToShow() => $"{AttackCounter + 6}/{HealthCounter + 6}";

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.CTHUN_ATTACK_BUFF && tag != GameTag.CTHUN_HEALTH_BUFF)
			return;

		if(value == 0)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if(controller == Game.Player.Id && IsPlayerCounter || controller == Game.Opponent.Id && !IsPlayerCounter)
		{
			if(tag == GameTag.CTHUN_ATTACK_BUFF)
				AttackCounter = value;
			else if(tag == GameTag.CTHUN_HEALTH_BUFF)
				HealthCounter = value;
		}
	}
}
