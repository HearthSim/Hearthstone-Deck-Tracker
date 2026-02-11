using System;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class UndeadAttackBonusCounter : NumericCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.NerubianDeathswarmer;
	public override string LocalizedName => LocUtil.Get("Counter_UndeadAttackBonus", useCardLanguage: true);
	public override string[] RelatedCards => new string[] {};

	public UndeadAttackBonusCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsBattlegroundsMatch
	                                     && Counter > 0
	                                     && Game.Player.Board.Any(e => e.Card.IsUndead());

	public override string[] GetCardsToDisplay()
	{
		return new []
		{
			HearthDb.CardIds.NonCollectible.Neutral.NerubianDeathswarmer,
			HearthDb.CardIds.NonCollectible.Neutral.AnubarakNerubianKing,
			HearthDb.CardIds.NonCollectible.Neutral.ChampionOfThePrimus,
			HearthDb.CardIds.NonCollectible.Neutral.Butchering,
			HearthDb.CardIds.NonCollectible.Neutral.ForsakenWeaver
		};
	}

	public override string ValueToShow() => $"+{Counter}";
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsBattlegroundsMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(tag == GameTag.ZONE
		   && (value == (int)Zone.PLAY || (value == (int)Zone.SETASIDE && prevValue == (int)Zone.PLAY))
		   && entity.Card.IsUndead())
		{
			OnCounterChanged();
		}

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Neutral.NerubianDeathswarmer_UndeadBonusAttackPlayerEnchantDnt)
			return;


		if(tag == GameTag.TAG_SCRIPT_DATA_NUM_1)
			Counter = value;

	}
}
