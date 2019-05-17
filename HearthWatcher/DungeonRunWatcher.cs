using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Objects;

namespace HearthWatcher
{
	public interface IGameDataProvider
	{
		bool InAiMatch { get; }
		bool InAdventureScreen { get; }
		string OpponentHeroId { get; }
		int OpponentHeroHealth { get; }
	}

	public class DungeonRunWatcher
	{
		private readonly IGameDataProvider _dataProvider;
		private readonly int _delay;
		public bool Running { get; private set; }
		private bool _watch;
		private List<int>[] _prevCards;
		private int[] _prevLootChoice;
		private int[] _prevTreasureChoice;

		public event Action<DungeonInfo> DungeonInfoChanged;
		public event Action<bool> DungeonRunMatchStarted;

		public DungeonRunWatcher(IGameDataProvider dataProvider, int delay = 500)
		{
			_dataProvider = dataProvider;
			_delay = delay;
		}

		public void Run()
		{
			_watch = true;
			if(!Running)
				Watch();
		}

		public void Stop() => _watch = false;

		private async void Watch()
		{
			Running = true;
			_prevCards = new List<int>[] { null, null, null, null, null };
			_prevLootChoice = new[] { 0, 0, 0, 0, 0 };
			_prevTreasureChoice = new[] { 0, 0, 0, 0, 0 };
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				if(await Update())
					break;
			}
			Running = false;
		}

		private readonly string[] _initialOpponents =
		{
			CardIds.NonCollectible.Rogue.BinkTheBurglarHeroic,
			CardIds.NonCollectible.Hunter.GiantRatHeroic,
			CardIds.NonCollectible.Hunter.WeeWhelpHeroic,

			CardIds.NonCollectible.Druid.AMangyWolfHeroic,
			CardIds.NonCollectible.Hunter.GobblesHeroic,
			CardIds.NonCollectible.Druid.RottoothHeroic,
		};

		public async Task<bool> Update()
		{
			if(_dataProvider.InAdventureScreen)
			{
				var shouldBreak = UpdateDungeonInfo();
				if(shouldBreak)
					return true;
			}
			else if(_dataProvider.InAiMatch && !string.IsNullOrEmpty(_dataProvider.OpponentHeroId))
			{
				if(Cards.All.TryGetValue(_dataProvider.OpponentHeroId, out var card))
				{
					if(new [] {CardSet.LOOTAPALOOZA, CardSet.GILNEAS, CardSet.DALARAN}.Contains(card.Set) && card.Id.Contains("BOSS") || card.Set == CardSet.TROLL && card.Id.EndsWith("h"))
					{
						if(card.Set == CardSet.DALARAN)
						{
							UpdateDungeonInfo();
							await Task.Delay(500);
						}
						var newRun = _initialOpponents.Contains(_dataProvider.OpponentHeroId)
									|| _dataProvider.OpponentHeroHealth == 10;
						DungeonRunMatchStarted?.Invoke(newRun);
						return true;
					}
				}
			}
			return false;
		}

		public bool UpdateDungeonInfo()
		{
			var dungeonInfo = Reflection.GetDungeonInfo();
			if(dungeonInfo != null)
			{
				for(var i = 0; i < dungeonInfo.Length; i++)
				{
					if(dungeonInfo[i] != null && (dungeonInfo[i].RunActive || dungeonInfo[i].SelectedDeckId != 0))
					{
						if(_prevCards[i] == null || !(dungeonInfo[i].DbfIds?.SequenceEqual(_prevCards[i]) ?? false)
							|| _prevLootChoice[i] != dungeonInfo[i].PlayerChosenLoot
							|| _prevTreasureChoice[i] != dungeonInfo[i].PlayerChosenTreasure)
						{
							_prevCards[i] = dungeonInfo[i].DbfIds?.ToList();
							_prevLootChoice[i] = dungeonInfo[i].PlayerChosenLoot;
							_prevTreasureChoice[i] = dungeonInfo[i].PlayerChosenTreasure;
							DungeonInfoChanged?.Invoke(dungeonInfo[i]);
						}
					}
					else
						_prevCards[i] = null;
				}

				if(_prevLootChoice.All(x => x > 0) && _prevTreasureChoice.All(x => x > 0))
					return true;
			}
			else
			{
				_prevCards = new List<int>[] { null, null, null, null, null };
			}
			return false;
		}
	}
}
