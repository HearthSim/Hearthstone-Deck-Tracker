using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class LeylineCostReductionCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.LeyWalker;
	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Mage.LeyWalker,
		HearthDb.CardIds.Collectible.Mage.TheArcanomicon,
	};

	public LeylineCostReductionCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsTraditionalHearthstoneMatch && Counter > 0;

	public override string[] GetCardsToDisplay()
	{
		return RelatedCards;
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

		if(value != (int)Zone.PLAY)
			return;

		if(entity.Card.Id == HearthDb.CardIds.NonCollectible.Mage.LeyWalker_UnblockLeylineToken)
		{
			Counter += 1;
		}
	}
}
