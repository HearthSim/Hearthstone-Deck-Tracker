using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class AncestralAutomatonCounter : StatsCounter
{
	public override bool IsBattlegroundsCounter => true;
	protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.AncestralAutomaton;

	public override string[] RelatedCards => new[]
	{
		HearthDb.CardIds.NonCollectible.Neutral.AncestralAutomaton,
		HearthDb.CardIds.NonCollectible.Neutral.AutoAssembler,
		HearthDb.CardIds.NonCollectible.Neutral.AutomatonPortrait,
	};

	// the magnetized Auto Assembler only shows up as its enchantment on the host minion
	private static readonly string[] Sources =
	{
		HearthDb.CardIds.NonCollectible.Neutral.AutomatonPortrait,
		HearthDb.CardIds.NonCollectible.Neutral.AutoAssembler,
		HearthDb.CardIds.NonCollectible.Neutral.AutoAssembler_AutoAssembler1,
		HearthDb.CardIds.NonCollectible.Neutral.AutoAssembler_AutoAssemblerEnchantment,
		HearthDb.CardIds.NonCollectible.Neutral.AutoAssembler_AutoAssembler2,
	};

	// unlike Eternal Legion, the Ancestral Technology enchantment carries no script data, so the
	// per-automaton buff is only in the card text
	private const int AttackPerAutomaton = 3;
	private const int HealthPerAutomaton = 2;

	private readonly int _automatonBaseAttack =
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.AncestralAutomaton)?.Attack ?? 3;

	private readonly int _automatonBaseHealth =
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.AncestralAutomaton)?.Health ?? 4;

	public AncestralAutomatonCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
		AttackCounter = _automatonBaseAttack;
		HealthCounter = _automatonBaseHealth;
	}

	public override bool ShouldShow() => Game.IsBattlegroundsMatch
	                                     && (AttackCounter > _automatonBaseAttack || HealthCounter > _automatonBaseHealth)
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

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Neutral.AncestralAutomaton_AncestralAutomatonPlayerEnchantDnt)
			return;

		if(tag == GameTag.TAG_SCRIPT_DATA_NUM_1)
		{
			// the next automaton counts every one summoned so far as an other one
			AttackCounter = _automatonBaseAttack + value * AttackPerAutomaton;
			HealthCounter = _automatonBaseHealth + value * HealthPerAutomaton;
		}
	}
}
