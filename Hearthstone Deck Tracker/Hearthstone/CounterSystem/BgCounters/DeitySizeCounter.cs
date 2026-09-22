using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class DeitySizeCounter : StatsCounter
{
	private const int ShowAboveStats = 10;

	public override bool IsBattlegroundsCounter => true;

	public override string LocalizedName => LocUtil.Get("Counter_Deity", useCardLanguage: true);

	// until the sigil tells us which Deity it holds, fall back to generic Aberration art
	protected override string? CardIdToShowInUI =>
		_deityCardId ?? HearthDb.CardIds.NonCollectible.Neutral.ATaleofKings_KingOfAberrationsTavernBrawl;

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
	                                     && (AttackCounter > ShowAboveStats || HealthCounter > ShowAboveStats);

	public override string[] GetCardsToDisplay() => RelatedCards;

	public override string ValueToShow() => $"{AttackCounter} / {HealthCounter}";

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsBattlegroundsMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(entity.CardId != HearthDb.CardIds.NonCollectible.Neutral.SecretDeityDnt)
			return;

		// the stats are the Deity's current total, not a bonus on top of the printed ones
		if(tag == GameTag.BACON_EVOLUTION_CARD_OVERWRITE_ATK)
			AttackCounter = value;

		if(tag == GameTag.BACON_EVOLUTION_CARD_OVERWRITE_HEALTH)
			HealthCounter = value;

		if(tag == GameTag.BACON_EVOLUTION_CARD_ID)
		{
			_deityCardId = Database.GetCardFromDbfId(value, false)?.Id;
			OnPropertyChanged(nameof(CardToShowInUi));
			OnPropertyChanged(nameof(CardAsset));
		}
	}
}
