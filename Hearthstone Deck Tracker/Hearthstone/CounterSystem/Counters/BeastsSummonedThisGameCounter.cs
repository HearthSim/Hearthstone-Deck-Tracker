using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

// Knight of the Wild / Frostsaber Matriarch: "Costs (1) less for each Beast you've summoned this game."
public class BeastsSummonedThisGameCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_SummonedBeasts", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Druid.KnightOfTheWildTGT;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Druid.KnightOfTheWildTGT,
		HearthDb.CardIds.Collectible.Druid.KnightOfTheWildWONDERS,
		HearthDb.CardIds.Collectible.Druid.FrostsaberMatriarch,
	};

	public BeastsSummonedThisGameCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;

		return IsPlayerCounter && InPlayerDeckOrKnown(RelatedCards);
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

		if(!entity.IsMinion)
			return;

		if(!entity.LatestCard.IsBeast())
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY)
			return;

		Counter++;
	}
}
