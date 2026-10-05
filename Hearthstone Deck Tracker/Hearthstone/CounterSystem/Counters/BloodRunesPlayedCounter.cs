using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

// G'huun: "Battlecry: Steal 6 Health split among the lowest Health enemies. Improved by Blood Runes you've played."
public class BloodRunesPlayedCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_BloodRunesPlayed", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Deathknight.Ghuun;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Deathknight.Ghuun,
	};

	public BloodRunesPlayedCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 5 && OpponentMayHaveRelevantCards();
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);
	}

	public override string ValueToShow() => Counter.ToString();

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

		var bloodRunes = entity.LatestCard.GetTag(GameTag.COST_BLOOD);
		if(bloodRunes <= 0)
			return;

		Counter += bloodRunes;
	}
}
