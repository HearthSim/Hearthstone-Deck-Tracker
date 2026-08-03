using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;
public class CounterManager
{
	protected IGame Game { get; }

	public readonly List<BaseCounter> PlayerCounters = new();
	public readonly List<BaseCounter> OpponentCounters = new();

	public CounterManager(IGame game)
	{
		Game = game;
	}

	private void Initialize()
	{
		foreach(var type in CounterTypeProvider.GetCounterTypes())
		{
			var playerCounter = (BaseCounter)Activator.CreateInstance(type,  true, Game);
			var opponentCounter = (BaseCounter)Activator.CreateInstance(type, false, Game);

			if(playerCounter != null)
			{
				playerCounter.CounterChanged += (sender, args) => NotifyCountersChanged();
				PlayerCounters.Add(playerCounter);
			}

			if(opponentCounter != null)
			{
				opponentCounter.CounterChanged += (sender, args) => NotifyCountersChanged();
				OpponentCounters.Add(opponentCounter);
			}
		}
	}

	public List<BaseCounter> GetVisibleCounters(bool controlledByPlayer)
	{
		var counters = controlledByPlayer ? PlayerCounters : OpponentCounters;
		return counters.Where(c => c.IsVisible()).ToList();
	}

	public List<BaseCounter> GetExampleCounters(bool controlledByPlayer)
	{
		var counters = controlledByPlayer ? PlayerCounters : OpponentCounters;
		return counters.Take(3).ToList();
	}

	public void HandleTagChange(GameTag tag, IHsGameState gameState, int id, int value, int prevValue)
	{
		if(!Game.Entities.TryGetValue(id, out var entity))
			return;

		foreach(var playerCounter in PlayerCounters)
		{
			playerCounter.HandleTagChange(tag, gameState, entity, value, prevValue);
		}

		foreach(var opponentCounter in OpponentCounters)
		{
			opponentCounter.HandleTagChange(tag, gameState, entity, value, prevValue);
		}
	}

	public void HandleChoicePicked(IHsCompletedChoice choice)
	{
		foreach(var playerCounter in PlayerCounters)
		{
			playerCounter.HandleChoicePicked(choice);
		}

		foreach(var opponentCounter in OpponentCounters)
		{
			opponentCounter.HandleChoicePicked(choice);
		}
	}

	public void Reset()
	{
		PlayerCounters.Clear();
		OpponentCounters.Clear();
		Initialize();

		NotifyCountersChanged();
	}

	public event EventHandler? CountersChanged;

	private void NotifyCountersChanged()
	{
		CountersChanged?.Invoke(this, EventArgs.Empty);
	}
}
