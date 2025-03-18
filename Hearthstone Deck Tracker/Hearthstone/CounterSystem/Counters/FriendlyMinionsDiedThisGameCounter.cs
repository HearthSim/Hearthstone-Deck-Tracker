using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class FriendlyMinionsDiedThisGameCounter: NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_FriendlyMinionsDiedThisGame", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.Aessina;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Mage.Aessina,
		HearthDb.CardIds.Collectible.Mage.Starsurge
	};

	public FriendlyMinionsDiedThisGameCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return OpponentMayHaveRelevantCards() && Counter >= 10;
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

		if(!Game.IsMulliganDone)
			return;

		if(!entity.IsMinion) return;

		if(tag != GameTag.ZONE) return;

		if(prevValue != (int)Zone.PLAY) return;

		if(value != (int)Zone.GRAVEYARD) return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter++;
	}
}
