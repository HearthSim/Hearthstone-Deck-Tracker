using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class AsteroidExtraDamageCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_AsteroidDamage", useCardLanguage: true);
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.Asteroid;
	public override string[] RelatedCards => new string[] {};

	public AsteroidExtraDamageCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => !Game.IsBattlegroundsMatch && Counter > 0;

	public override string[] GetCardsToDisplay()
	{
		return new []
		{
			HearthDb.CardIds.NonCollectible.Neutral.Asteroid
		};
	}

	public override bool IsDisplayValueLong => true;

	public override string ValueToShow() {
		return string.Format(LocUtil.Get("Counter_AsteroidDamage_Damage", useCardLanguage: true), 2 + Counter);
	}

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(entity.IsControlledBy(Game.Player.Id) == IsPlayerCounter)
		{
			if((int)tag == 3559)
			{
				Counter = value;
			}
		}
	}
}
