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
	}
	public class DungeonRunWatcher
	{
		private readonly IGameDataProvider _dataProvider;
		private readonly int _delay;
		public bool Running { get; private set; }
		private bool _watch;
		private List<int> _prevCards;
		private int _prevLootChoice;
		private int _prevTreasureChoice;

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
			_prevCards = null;
			_prevLootChoice = 0;
			_prevTreasureChoice = 0;
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				if(Update())
					break;
			}
			Running = false;
		}

		private readonly string[] _initialOpponents =
		{
			CardIds.NonCollectible.Rogue.BinkTheBurglarHeroic,
			CardIds.NonCollectible.Hunter.GiantRatHeroic,
			CardIds.NonCollectible.Hunter.WeeWhelpHeroic
		};

		public bool Update()
		{
			if(_dataProvider.InAdventureScreen)
			{
				var dungeonInfo = Reflection.GetDungeonInfo();
				if(dungeonInfo?.RunActive ?? false)
				{
					if(_prevCards == null || !dungeonInfo.DbfIds.SequenceEqual(_prevCards)
						|| _prevLootChoice != dungeonInfo.PlayerChosenLoot
						|| _prevTreasureChoice != dungeonInfo.PlayerChosenTreasure)
					{
						_prevCards = dungeonInfo.DbfIds.ToList();
						_prevLootChoice = dungeonInfo.PlayerChosenLoot;
						_prevTreasureChoice = dungeonInfo.PlayerChosenTreasure;
						DungeonInfoChanged?.Invoke(dungeonInfo);
						if(_prevLootChoice > 0 && _prevTreasureChoice > 0)
							return true;
					}
				}
				else
					_prevCards = null;
			}
			else if(_dataProvider.InAiMatch && !string.IsNullOrEmpty(_dataProvider.OpponentHeroId))
			{
				if(Cards.All.TryGetValue(_dataProvider.OpponentHeroId, out var card))
				{
					if(card.Set == CardSet.LOOTAPALOOZA && card.Id.Contains("BOSS"))
					{
						var newRun = _initialOpponents.Contains(_dataProvider.OpponentHeroId);
						DungeonRunMatchStarted?.Invoke(newRun);
						return true;
					}
				}
			}
			return false;
		}
	}
}
