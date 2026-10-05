using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

// Y'Shaarj, Titan Bane: "Old God. Battlecry: If you've spent 30 Mana on minions this game, gain Immune for 1 turn."
public class ManaSpentOnMinionsThisGameCounter : NumericCounter
{
	private const int YshaarjThreshold = 30;

	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.YshaarjTitanBane;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Neutral.YshaarjTitanBane,
	};

	public ManaSpentOnMinionsThisGameCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);

		return Counter > 20 && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);
	}

	public override string ValueToShow() => $"{System.Math.Min(Counter, YshaarjThreshold)}/{YshaarjThreshold}";

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.NUM_RESOURCES_SPENT_THIS_GAME)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(gameState.CurrentBlock?.Type != "PLAY")
			return;

		var playedCard = new Card(gameState.CurrentBlock.CardId ?? "");
		if(playedCard.TypeEnum != CardType.MINION)
			return;

		var spent = value - prevValue;
		if(spent <= 0)
			return;

		Counter += spent;
	}
}
