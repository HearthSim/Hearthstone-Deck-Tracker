using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class HeraldCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_Herald", useCardLanguage: true);

	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Invalid.EnvoyOfTheEnd;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Deathknight.ObsessiveTechnician,
		HearthDb.CardIds.Collectible.Deathknight.ExperimentalAnimation,
		HearthDb.CardIds.Collectible.Deathknight.ArisenOnyxia,

		HearthDb.CardIds.Collectible.Demonhunter.FelInfusion,
		HearthDb.CardIds.Collectible.Demonhunter.ArmoredBloodletter,
		HearthDb.CardIds.Collectible.Demonhunter.AzsharaOceanLord,

		HearthDb.CardIds.Collectible.Rogue.RiteOfTwilight,
		HearthDb.CardIds.Collectible.Rogue.ManiacalFollower,
		HearthDb.CardIds.Collectible.Rogue.Sinestra,

		HearthDb.CardIds.Collectible.Shaman.SkywallSentinel,
		HearthDb.CardIds.Collectible.Shaman.RitualOfPower,
		HearthDb.CardIds.Collectible.Shaman.AlakirLordOfStorms,

		HearthDb.CardIds.Collectible.Warlock.ShadowswornDisciple,
		HearthDb.CardIds.Collectible.Warlock.ShrineOfTwilight,
		HearthDb.CardIds.Collectible.Warlock.ChogallMastermind,

		HearthDb.CardIds.Collectible.Warrior.CataclysmicWarAxe,
		HearthDb.CardIds.Collectible.Warrior.ScorchingRavager,
		HearthDb.CardIds.Collectible.Warrior.RagnarosTheGreatFire,

		HearthDb.CardIds.Collectible.Invalid.EnvoyOfTheEnd,
		HearthDb.CardIds.Collectible.Invalid.Ultraxion,
		HearthDb.CardIds.Collectible.Invalid.DeathwingWorldbreakerHeroic,
	};

	public HeraldCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 0 && OpponentMayHaveRelevantCards();
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

		if(tag != GameTag.HERALD_COLOSSAL_AMOUNT)
			return;

		if(value == 0)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if(controller == Game.Player.Id && IsPlayerCounter || controller == Game.Opponent.Id && !IsPlayerCounter)
		{
			Counter = value;
		}
	}
}
