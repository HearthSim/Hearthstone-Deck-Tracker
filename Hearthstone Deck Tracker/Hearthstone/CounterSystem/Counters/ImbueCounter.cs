using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class ImbueCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.MalorneTheWaywatcher;
	public override string LocalizedName => LocUtil.Get("Counter_Imbue", useCardLanguage: true);

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.NonCollectible.Druid.DreamboundDisciple_BlessingOfTheGolem,
		HearthDb.CardIds.NonCollectible.Hunter.BlessingOfTheWolf,
		HearthDb.CardIds.NonCollectible.Mage.BlessingOfTheWisp,
		HearthDb.CardIds.NonCollectible.Paladin.BlessingOfTheDragon,
		HearthDb.CardIds.NonCollectible.Priest.LunarwingMessenger_BlessingOfTheMoon,
		HearthDb.CardIds.NonCollectible.Shaman.BlessingOfTheWind,
		HearthDb.CardIds.Collectible.Neutral.MalorneTheWaywatcher,
	};

	public ImbueCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		return Counter > 0;
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

		if(tag != (GameTag)3527)
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
