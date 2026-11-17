using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

// Y'Shaarj, Titan Bane: "Old God. Battlecry: If you've spent 30 Mana on minions this game, gain Immune for 1 turn."
// Only Mana counts: Health and Corpses paid for minions no longer count toward the 30 (fixed in game).
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

	// Mana payment happens directly inside the minion's PLAY block; Battlecries run in child POWER
	// blocks and are ignored.
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.NUM_RESOURCES_SPENT_THIS_GAME)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(!IsMinionPlayBlock(gameState))
			return;

		var spent = value - prevValue;
		if(spent <= 0)
			return;

		Counter += spent;
	}

	private static bool IsMinionPlayBlock(IHsGameState gameState)
	{
		if(gameState.CurrentBlock?.Type != "PLAY")
			return false;

		var playedCard = new Card(gameState.CurrentBlock.CardId ?? "");
		return playedCard.TypeEnum == CardType.MINION;
	}
}
