using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class SpellsPlayedForNagasCounter : NumericCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.Thaumaturgist;
	public override string LocalizedName => LocUtil.Get("Counter_PlayedSpells", useCardLanguage: true);
	public override string[] RelatedCards => new []
	{
		HearthDb.CardIds.NonCollectible.Neutral.Thaumaturgist,
		HearthDb.CardIds.NonCollectible.Neutral.ArcaneCannoneer,
		HearthDb.CardIds.NonCollectible.Neutral.ShowyCyclist,
		HearthDb.CardIds.NonCollectible.Neutral.Groundbreaker
	};

	private readonly string[] _goldenVersions = {
		HearthDb.CardIds.NonCollectible.Neutral.Thaumaturgist_Thaumaturgist,
		HearthDb.CardIds.NonCollectible.Neutral.ArcaneCannoneer_ArcaneCannoneer,
		HearthDb.CardIds.NonCollectible.Neutral.ShowyCyclist_ShowyCyclist,
		HearthDb.CardIds.NonCollectible.Neutral.Groundbreaker_Groundbreaker
	};

	public SpellsPlayedForNagasCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsBattlegroundsMatch
	                                     && Counter > 1
	                                     && Game.Player.Board.Any(e => RelatedCards.Concat(_goldenVersions).Any(rc => e.CardId == rc));

	public override string[] GetCardsToDisplay() => RelatedCards;

	public override string ValueToShow() => $"{1 + (Counter / 4)}  ({Counter % 4}/4)";
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


		if(tag == (GameTag)3809)
		{
			Counter = value;
		}
	}
}
