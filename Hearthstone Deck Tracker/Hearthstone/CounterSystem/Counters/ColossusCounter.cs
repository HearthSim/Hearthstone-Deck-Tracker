using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class ColossusCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.Colossus;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Mage.Colossus,
	};

	public ColossusCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);

		var couldBeGenerated = Game.Opponent.PlayerEntities.Any(e =>
		{
			if(e is { IsInHand: false, IsInPlay: false, IsInDeck: false })
				return false;

			if(!e.Info.Created)
				return false;

			if(e.HasCardId && e.CardId != HearthDb.CardIds.Collectible.Mage.Colossus)
				return false;

			var creatorId = e.Info.GetCreatorId();
			if(creatorId <= 0 || !Game.Entities.TryGetValue(creatorId, out var creator))
				return false;
			return creator.CardId == HearthDb.CardIds.Collectible.Priest.Mothership;
		});

		return (Counter > 1 && OpponentMayHaveRelevantCards()) || couldBeGenerated;
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);
	}

	public override string ValueToShow() => string.Format(LocUtil.Get("Counter_AsteroidDamage_Damage", useCardLanguage: true), $"2x {Counter + 1}");

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);
		if(!((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter)))
			return;

		if(DiscountIfCantPlay(tag, value, entity))
			return;

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY && value != (int)Zone.SECRET)
			return;

		if(!entity.IsSpell)
			return;

		if(!entity.HasTag(GameTag.PROTOSS))
			return;

		LastEntityToCount = entity;
		Counter++;
	}
}
