using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class EternalKnightCounter : StatsCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.EternalKnight;

	public override string[] RelatedCards => new[]
	{
		HearthDb.CardIds.NonCollectible.Neutral.EternalKnight,
		HearthDb.CardIds.NonCollectible.Neutral.EternalSummoner,
	};

	private static readonly string[] Sources =
	{
		HearthDb.CardIds.NonCollectible.Neutral.EternalSummoner,
		HearthDb.CardIds.NonCollectible.Neutral.EternalSummoner_EternalSummoner,
	};

	private readonly int _knightBaseAttack =
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.EternalKnight)?.Attack ?? 4;

	private readonly int _knightBaseHealth =
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.EternalKnight)?.Health ?? 2;

	private readonly int _attackPerDeath = EternalLegionTag(GameTag.TAG_SCRIPT_DATA_NUM_1, 1);
	private readonly int _healthPerDeath = EternalLegionTag(GameTag.TAG_SCRIPT_DATA_NUM_2, 1);

	private static int EternalLegionTag(GameTag tag, int fallback)
	{
		var value = Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.EternalKnight_EternalLegion)
			?.Data?.Entity.GetTag(tag) ?? 0;
		return value > 0 ? value : fallback;
	}

	public EternalKnightCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
		AttackCounter = _knightBaseAttack;
		HealthCounter = _knightBaseHealth;
	}

	public override bool ShouldShow() => Game.IsBattlegroundsMatch
	                                     && (AttackCounter > _knightBaseAttack || HealthCounter > _knightBaseHealth)
	                                     && Game.Player.Board.Any(e => Sources.Contains(e.CardId));

	public override string[] GetCardsToDisplay() => RelatedCards;

	public override string ValueToShow() => $"{AttackCounter} / {HealthCounter}";

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsBattlegroundsMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(tag == GameTag.ZONE
		   && (value == (int)Zone.PLAY || (value == (int)Zone.SETASIDE && prevValue == (int)Zone.PLAY))
		   && Sources.Contains(entity.CardId))
		{
			OnCounterChanged();
		}

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Neutral.EternalKnight_EternalKnightPlayerEnchantDnt)
			return;

		if(tag == GameTag.TAG_SCRIPT_DATA_NUM_1)
		{
			AttackCounter = _knightBaseAttack + value * _attackPerDeath;
			HealthCounter = _knightBaseHealth + value * _healthPerDeath;
		}
	}
}
