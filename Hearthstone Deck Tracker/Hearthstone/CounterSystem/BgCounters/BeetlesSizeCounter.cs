using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class BeetlesSizeCounter : StatsCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.BoonofBeetles_BeetleToken1;
	public override string[] RelatedCards => new []
	{
		HearthDb.CardIds.NonCollectible.Neutral.BoonofBeetles_BeetleToken1,
		HearthDb.CardIds.NonCollectible.Neutral.BuzzingVermin,
		HearthDb.CardIds.NonCollectible.Neutral.ForestRover,
		HearthDb.CardIds.NonCollectible.Neutral.TurquoiseSkitterer,
		HearthDb.CardIds.NonCollectible.Neutral.RunedProgenitor,
		HearthDb.CardIds.NonCollectible.Neutral.NestSwarmer,
		HearthDb.CardIds.NonCollectible.Neutral.SilkyShimmermoth
	};

	private IEnumerable<string> RelatedCardsWithTriples => RelatedCards.Concat(RelatedCards.Select(HearthDb.Cards.TryGetTripleId));


	private readonly int _beetleBaseAttack =
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.BoonofBeetles_BeetleToken1)?.Attack ?? 1;

	private readonly int _beetleBaseHealth =
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.BoonofBeetles_BeetleToken1)?.Health ?? 1;

	public BeetlesSizeCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsBattlegroundsMatch
	                                     && (AttackCounter > _beetleBaseAttack || HealthCounter > _beetleBaseHealth)
	                                     && Game.Player.Board.Any(e => RelatedCardsWithTriples.Any(rc => e.CardId == rc));

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
		   && RelatedCards.Contains(entity.CardId))
		{
			OnCounterChanged();
		}

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Neutral.RunedProgenitor_BeetleArmyPlayerEnchantDnt)
			return;

		if(tag == GameTag.TAG_SCRIPT_DATA_NUM_1)
			AttackCounter = _beetleBaseAttack + value;

		if(tag == GameTag.TAG_SCRIPT_DATA_NUM_2)
			HealthCounter = _beetleBaseHealth + value;
	}
}
