using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class ElementalTurnSequenceCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_ElementalTurnSequence", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.Lamplighter;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Neutral.Lamplighter,
		HearthDb.CardIds.Collectible.Neutral.AzeriteGiant,
		HearthDb.CardIds.Collectible.Mage.ElementalAllies,
		HearthDb.CardIds.Collectible.Mage.OverflowSurger,
		HearthDb.CardIds.Collectible.Shaman.SkarrTheCatastrophe
	};

	public ElementalTurnSequenceCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	private bool _shownBefore;

	public override bool ShouldShow()
	{
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);

		if(Counter > 2 && OpponentMayHaveRelevantCards())
			_shownBefore = true;

		return (Counter > 2 && OpponentMayHaveRelevantCards()) || _shownBefore;
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.Class);
	}

	public override string ValueToShow() => Counter.ToString();

	private int LastPlayedTurn { get; set; }
	private bool PlayedThisTurn { get; set; }
	public void HandleElementalPlayed(IGame game, IHsGameState gameState, Entity entity)
	{
		var isCurrentController = IsPlayerCounter ? entity.IsControlledBy(game.Player.Id)
			: entity.IsControlledBy(game.Opponent.Id);

		if(!isCurrentController)
			return;

		if(!entity.Card.IsElemental())
			return;

		if(PlayedThisTurn)
			return;

		var turnNumber = gameState.GetTurnNumber();

		if(turnNumber == LastPlayedTurn + 1 || Counter == 0)
		{
			LastPlayedTurn = turnNumber;
			PlayedThisTurn = true;
			Counter++;
		}
	}

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag == GameTag.ZONE && gameState.CurrentBlock?.Type == "PLAY")
			HandleElementalPlayed(Game, gameState, entity);

		if(tag != GameTag.CURRENT_PLAYER)
			return;

		var isNewEnemyTurn = (IsPlayerCounter ? Game.OpponentEntity?.HasTag(GameTag.CURRENT_PLAYER) : Game.PlayerEntity?.HasTag(GameTag.CURRENT_PLAYER)) ?? false;
		var isNewFriendlyTurn = (IsPlayerCounter ? Game.PlayerEntity?.HasTag(GameTag.CURRENT_PLAYER) : Game.OpponentEntity?.HasTag(GameTag.CURRENT_PLAYER)) ?? false;

		if(!isNewEnemyTurn && !isNewFriendlyTurn)
			return;

		if(isNewFriendlyTurn)
			PlayedThisTurn = false;

		if(isNewEnemyTurn && !PlayedThisTurn)
			Counter = 0;
	}
}
