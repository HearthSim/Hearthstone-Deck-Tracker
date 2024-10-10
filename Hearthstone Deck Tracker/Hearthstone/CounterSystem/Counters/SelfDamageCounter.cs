using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class SelfDamageCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_SelfDamage", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.PartyPlannerVona;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Warlock.PartyPlannerVona,
		HearthDb.CardIds.Collectible.Warlock.ImprisonedHorror
	};

	public SelfDamageCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 7 && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.Class);
	}

	public override string ValueToShow() => Counter.ToString();

	private int PreDamage { get; set; }

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(!entity.IsHero)
			return;

		var isEnemyTurn = (IsPlayerCounter ? Game.OpponentEntity?.HasTag(GameTag.CURRENT_PLAYER) : Game.PlayerEntity?.HasTag(GameTag.CURRENT_PLAYER)) ?? false;
		var isFriendlyTurn = (IsPlayerCounter ? Game.PlayerEntity?.HasTag(GameTag.CURRENT_PLAYER) : Game.OpponentEntity?.HasTag(GameTag.CURRENT_PLAYER)) ?? false;

		if(isEnemyTurn)
			return;

		if(!isFriendlyTurn)
			return;

		if(!IsPlayerCounter && entity.IsControlledBy(Game.Player.Id))
			return;

		if(IsPlayerCounter && entity.IsControlledBy(Game.Opponent.Id))
			return;

		if(tag == GameTag.PREDAMAGE)
		{
			if(value == 0)
				return;

			if(prevValue != 0)
				return;

			PreDamage = value;
			return;
		}

		if(tag == GameTag.DAMAGE)
		{
			if(PreDamage + prevValue != value)
				return;

			Counter += PreDamage;
			PreDamage = 0;
		}
	}
}
