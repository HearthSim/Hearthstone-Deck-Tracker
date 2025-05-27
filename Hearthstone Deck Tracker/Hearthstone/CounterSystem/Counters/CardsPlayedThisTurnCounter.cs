using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class CardsPlayedThisTurnCounter: NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_CardsPlayedThisTurn", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.EdwinVancleef;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Rogue.EdwinVancleef,
		HearthDb.CardIds.Collectible.Rogue.EdwinVancleefVanilla,
		HearthDb.CardIds.Collectible.Rogue.PrizePlunderer,
		HearthDb.CardIds.Collectible.Rogue.SinstoneGraveyard,
		HearthDb.CardIds.Collectible.Rogue.SinstoneGraveyardCorePlaceholder,
		HearthDb.CardIds.Collectible.Rogue.ShadowSculptor,
		HearthDb.CardIds.Collectible.Rogue.EverburningPhoenix,
		HearthDb.CardIds.Collectible.Rogue.Biteweed,
		HearthDb.CardIds.Collectible.Rogue.NecrolordDraka,
		HearthDb.CardIds.Collectible.Rogue.NecrolordDrakaCorePlaceholder,
		HearthDb.CardIds.Collectible.Rogue.SpectralPillager,
		HearthDb.CardIds.Collectible.Rogue.SpectralPillagerCorePlaceholder,
		HearthDb.CardIds.Collectible.Rogue.ScribblingStenographer,
		HearthDb.CardIds.Collectible.Rogue.ScribblingStenographerCorePlaceholder,
		HearthDb.CardIds.Collectible.Neutral.FrostwolfWarmaster,
	};

	public CardsPlayedThisTurnCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		return IsPlayerCounter && InPlayerDeckOrKnown(RelatedCards);
	}

	public override string[] GetCardsToDisplay()
	{
		return GetCardsInDeckOrKnown(RelatedCards).ToArray();
	}

	public override string ValueToShow() => Counter.ToString();
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(DiscountIfCantPlay(tag, value, entity))
			return;

		if(tag == GameTag.NUM_TURNS_IN_PLAY)
		{
			Counter = 0;
			return;
		}

		if(tag != GameTag.ZONE)
			return;

		if(prevValue != (int)Zone.HAND)
			return;

		if((value is (int)Zone.PLAY or (int)Zone.SECRET) && gameState.CurrentBlock?.Type == "PLAY")
		{
			LastEntityToCount = entity;
			Counter++;
		}
	}
}
