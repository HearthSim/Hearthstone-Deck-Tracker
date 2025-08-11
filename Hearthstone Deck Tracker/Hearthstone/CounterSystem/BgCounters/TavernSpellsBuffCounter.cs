using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class TavernSpellsBuffCounter : StatsCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.ShinyRing;
	public override string LocalizedName => LocUtil.Get("Counter_TavernSpellsBuff", useCardLanguage: true);
	public override string[] RelatedCards => new string[] { };

	public TavernSpellsBuffCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsBattlegroundsMatch && (AttackCounter > 1 || HealthCounter > 1);

	public override string[] GetCardsToDisplay()
	{
		return new[]
		{
			HearthDb.CardIds.NonCollectible.Neutral.IntrepidBotanist,
			HearthDb.CardIds.NonCollectible.Neutral.TranquilMeditative,
			HearthDb.CardIds.NonCollectible.Neutral.ShoalfinMystic,
			HearthDb.CardIds.NonCollectible.Neutral.Humongozz,
			HearthDb.CardIds.NonCollectible.Neutral.FelfireConjurer,
			HearthDb.CardIds.NonCollectible.Neutral.BlueWhelp,
			HearthDb.CardIds.NonCollectible.Neutral.FriendlyGeist
		};
	}

	public override string ValueToShow() => $"+{AttackCounter} / +{HealthCounter}";

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if (!Game.IsBattlegroundsMatch)
			return;

		if (entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if (!entity.IsPlayer)
			return;

		if (tag == GameTag.TAVERN_SPELL_ATTACK_INCREASE)
		{
			AttackCounter = value;
			OnCounterChanged();
		}
		else if (tag == GameTag.TAVERN_SPELL_HEALTH_INCREASE)
		{
			HealthCounter = value;
			OnCounterChanged();
		}
	}
}
