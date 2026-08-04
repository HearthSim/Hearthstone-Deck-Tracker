using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

// Dragoncaller Alanna: "Battlecry: Summon a 5/5 Dragon for each spell you cast this game
// that costs (5) or more." The value is the number of dragons the battlecry would summon.
public class ExpensiveSpellsPlayedCounter : NumericCounter
{
	private const int MinimumCost = 5;

	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.DragoncallerAlanna;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Mage.DragoncallerAlanna,
	};

	public ExpensiveSpellsPlayedCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;

		return IsPlayerCounter && InPlayerDeckOrKnown(RelatedCards);
	}

	public override string[] GetCardsToDisplay() =>
		IsPlayerCounter
			? GetCardsInDeckOrKnown(RelatedCards).ToArray()
			: FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY && value != (int)Zone.SECRET)
			return;

		if(gameState.CurrentBlock?.Type != "PLAY")
			return;

		if(!entity.IsSpell)
			return;

		var cost = entity.LatestCard.Cost;

		if(cost < MinimumCost)
			return;

		Counter++;
	}
}
