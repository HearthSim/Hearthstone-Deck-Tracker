using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class DeitySizeCounter : StatsCounter
{
	private const int MinStatsToShow = 25;
	private const int MinAberrationsToShow = 3;

	public override bool IsBattlegroundsCounter => true;

	public override string LocalizedName => LocUtil.Get("Counter_Deity", useCardLanguage: true);

	// the sigil only appears a minute into the game, so until then we go by the lobby's Old God,
	// and by generic Aberration art before the game entity even has that
	protected override string? CardIdToShowInUI =>
		_deityCardId ?? Game.BattlegroundsGlobalOldGod?.Id
		?? HearthDb.CardIds.NonCollectible.Neutral.ATaleofKings_KingOfAberrationsTavernBrawl;

	public override string[] RelatedCards => new[]
	{
		HearthDb.CardIds.NonCollectible.Neutral.Joyous,
		HearthDb.CardIds.NonCollectible.Neutral.DriftingSacrifice,
		HearthDb.CardIds.NonCollectible.Neutral.ViciousMindslasher,
		HearthDb.CardIds.NonCollectible.Neutral.BrainRotter,
		HearthDb.CardIds.NonCollectible.Neutral.CutthroatKthir,
		HearthDb.CardIds.NonCollectible.Neutral.FacelessConverter,
		HearthDb.CardIds.NonCollectible.Neutral.TheShadowOfDoubt,
		HearthDb.CardIds.NonCollectible.Neutral.HarbingerAphlass,
		HearthDb.CardIds.NonCollectible.Neutral.ShaOfFear,
		HearthDb.CardIds.NonCollectible.Neutral.EnergizingChamber,
	};

	private string? _deityCardId;

	public DeitySizeCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow() => Game.IsBattlegroundsMatch
	                                     && (AttackCounter >= MinStatsToShow || HealthCounter >= MinStatsToShow
	                                         || (HasValue && AberrationsOnBoard() >= MinAberrationsToShow));

	private int AberrationsOnBoard()
	{
		var board = IsPlayerCounter ? Game.Player.Board : Game.Opponent.Board;
		return board.Count(IsAberration);
	}

	// the static race misses a minion turned into an Aberration by an enchantment (Faceless Converter),
	// which only the live CARDRACE tag carries
	private static bool IsAberration(Entity entity)
	{
		if(!entity.IsMinion)
			return false;
		var liveRace = (Race)entity.GetTag(GameTag.CARDRACE);
		return entity.Card.IsAberration() || liveRace == Race.ABERRATION || liveRace == Race.ALL;
	}

	public override string[] GetCardsToDisplay() => RelatedCards;

	public override string ValueToShow() => $"{AttackCounter} / {HealthCounter}";

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsBattlegroundsMatch)
			return;

		// the game entity has no controller, so this has to come before the controller check
		if(tag == GameTag.BACON_GLOBAL_OLD_GOD_DBID)
		{
			NotifyDeityChanged();
			return;
		}

		if(tag == GameTag.BACON_OLD_GOD_ATTACK || tag == GameTag.BACON_OLD_GOD_HEALTH)
		{
			HandleDeitySize(tag, entity, value);
			return;
		}

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(entity.CardId != HearthDb.CardIds.NonCollectible.Neutral.SecretDeityDnt)
			return;

		if(tag == GameTag.BACON_EVOLUTION_CARD_ID)
		{
			_deityCardId = Database.GetCardFromDbfId(value, false)?.Id;
			NotifyDeityChanged();
		}
	}

	// the size lives on the player entity rather than the sigil, and is the Deity's current total
	// rather than a bonus on top of the printed stats
	private void HandleDeitySize(GameTag tag, Entity entity, int value)
	{
		if(entity.Id != (IsPlayerCounter ? Game.PlayerEntity : Game.OpponentEntity)?.Id)
			return;

		// the opponent entity only carries a size while we are facing them and drops back to 0 once
		// their board is hidden again, so keep the last one we saw
		if(value == 0 && !IsPlayerCounter)
			return;

		if(tag == GameTag.BACON_OLD_GOD_ATTACK)
			AttackCounter = value;
		else
			HealthCounter = value;
	}

	private void NotifyDeityChanged()
	{
		OnPropertyChanged(nameof(CardToShowInUi));
		OnPropertyChanged(nameof(CardAsset));
	}
}
