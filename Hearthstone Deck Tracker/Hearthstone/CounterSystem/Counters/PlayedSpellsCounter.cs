using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class PlayedSpellsCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_PlayedSpells", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.YoggSaronHopesEnd;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Neutral.YoggSaronHopesEnd,
		HearthDb.CardIds.Collectible.Neutral.ArcaneGiant,
		HearthDb.CardIds.Collectible.Priest.GraveHorror,
		HearthDb.CardIds.Collectible.Druid.UmbralOwl,
		HearthDb.CardIds.Collectible.Druid.UmbralOwlCorePlaceholder,
		HearthDb.CardIds.Collectible.Neutral.YoggSaronMasterOfFate,
		HearthDb.CardIds.Collectible.Demonhunter.SaroniteShambler,
		HearthDb.CardIds.Collectible.Druid.ContaminatedLasher,
		HearthDb.CardIds.Collectible.Mage.MeddlesomeServant,
		HearthDb.CardIds.Collectible.Neutral.PrisonBreaker,
	};

	public PlayedSpellsCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 7 && OpponentMayHaveRelevantCards(true);
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.Class);
	}

	public override string ValueToShow() => Counter.ToString();

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY && value != (int)Zone.SECRET)
			return;

		if(!entity.IsSpell)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter++;
	}
}
