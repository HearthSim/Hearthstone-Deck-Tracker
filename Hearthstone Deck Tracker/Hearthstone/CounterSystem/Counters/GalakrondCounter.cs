using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class GalakrondCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.GalakrondTheNightmare;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Rogue.GalakrondTheNightmare,
		HearthDb.CardIds.Collectible.Shaman.GalakrondTheTempest,
		HearthDb.CardIds.Collectible.Warlock.GalakrondTheWretched,
		HearthDb.CardIds.Collectible.Priest.GalakrondTheUnspeakable,
		HearthDb.CardIds.Collectible.Warrior.GalakrondTheUnbreakable
	};

	public GalakrondCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 0 && OpponentMayHaveRelevantCards();
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

		if(tag != GameTag.INVOKE_COUNTER)
			return;

		if(value == 0)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if(controller == Game.Player.Id && IsPlayerCounter || controller == Game.Opponent.Id && !IsPlayerCounter)
			Counter = value;
	}
}
