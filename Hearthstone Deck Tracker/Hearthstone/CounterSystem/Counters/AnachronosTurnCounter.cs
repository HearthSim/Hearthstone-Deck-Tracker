using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class AnachronosTurnCounter : NumericCounter
{
	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.Anachronos;

	public override string[] RelatedCards => new string[] {};
	public AnachronosTurnCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public int AnachronosEnchantmentsInPlay { get; set; }

	private int _anachronosPowerBlockId = -1;

	private readonly List<string> _opponentMinions = new();

	private readonly List<string> _playerMinions = new();

	private string OpponentHero => Game.CurrentGameStats?.OpponentHeroCardId ?? "";

	private string PlayerHero => Game.CurrentGameStats?.PlayerHeroCardId ?? "";

	private List<string> _cards()
	{
		var retval = new List<string>();

		if(_opponentMinions.Count > 0)
		{
			retval.AddRange(_opponentMinions);
			retval.Add(OpponentHero);
		}

		if(_playerMinions.Count > 0)
		{
			retval.AddRange(_playerMinions);
			retval.Add(PlayerHero);
		}

		return retval;
	}

	public override bool ShouldShow() => Game.IsTraditionalHearthstoneMatch && AnachronosEnchantmentsInPlay > 0;

	public override string[] GetCardsToDisplay()
	{
		var retval = new List<string>();

		if(_opponentMinions.Count > 0)
		{
			retval.Add(OpponentHero);
			retval.AddRange(_opponentMinions);
		}

		if(_playerMinions.Count > 0)
		{
			retval.Add(PlayerHero);
			retval.AddRange(_playerMinions);
		}

		return retval.ToArray();
	}

	public override string ValueToShow() => $"{Counter.ToString()} / 2";
	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(tag == GameTag.ZONE  &&
		   entity.Card.Id == HearthDb.CardIds.NonCollectible.Paladin.Anachronos_TimeTravelEnchantment &&
		   entity.GetTag(GameTag.CONTROLLER) == (IsPlayerCounter ? Game.Player.Id : Game.Opponent.Id))
		{
			if(value == (int)Zone.PLAY)
			{
				AnachronosEnchantmentsInPlay++;
				Counter = 0;
				OnCounterChanged();

				if(_anachronosPowerBlockId == -1)
				{
					_anachronosPowerBlockId = gameState.CurrentBlock?.Id ?? -1;
				}
			}
			else if(value is (int)Zone.GRAVEYARD)
			{
				AnachronosEnchantmentsInPlay--;
				OnCounterChanged();

				_anachronosPowerBlockId = -1;
				_playerMinions.Clear();
				_opponentMinions.Clear();
			}
		}

		// handling only 1 effect at time for player for now
		if(AnachronosEnchantmentsInPlay >= 2)
			return;

		HandleMinions(tag, gameState, entity, value, prevValue);

		if(entity.Card.Id != HearthDb.CardIds.NonCollectible.Paladin.Anachronos_TimeTravelEnchantment)
			return;

		if(tag != GameTag.TAG_SCRIPT_DATA_NUM_2)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if((controller == Game.Player.Id && IsPlayerCounter) || (controller == Game.Opponent.Id && !IsPlayerCounter))
			Counter = value;
	}

	private void HandleMinions(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(gameState.CurrentBlock?.Id != _anachronosPowerBlockId)
			return;

		if(!entity.IsMinion)
			return;

		if(tag != GameTag.ZONE)
			return;

		if(prevValue != (int)Zone.PLAY)
			return;

		if(value != (int)Zone.SETASIDE)
			return;

		if(entity.CardId == null)
			return;

		var controller = entity.GetTag(GameTag.CONTROLLER);

		if(controller == Game.Player.Id)
		{
			_playerMinions.Add(entity.CardId);
			OnCounterChanged();
		}
		else if(controller == Game.Opponent.Id)
		{
			_opponentMinions.Add(entity.CardId);
			OnCounterChanged();
		}

	}
}
