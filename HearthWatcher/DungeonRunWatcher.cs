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

	public class DungeonRunWatcher : PollingWatcher
	{
		private readonly IGameDataProvider _dataProvider;
		private List<int>?[] _prevCards = new List<int>?[7];
		private int[] _prevLootChoice = new int[7];
		private int[] _prevTreasureChoice = new int[7];

		public event Action<DungeonInfo>? DungeonInfoChanged;
		public event Action<bool, CardSet>? DungeonRunMatchStarted;

		public DungeonRunWatcher(IGameDataProvider dataProvider, int delay = 500) : base(delay)
		{
			_dataProvider = dataProvider;
		}

		protected override void OnLoopStart()
		{
			_prevCards = new List<int>?[] { null, null, null, null, null, null, null };
			_prevLootChoice = new[] { 0, 0, 0, 0, 0, 0, 0 };
			_prevTreasureChoice = new[] { 0, 0, 0, 0, 0, 0, 0 };
		}

		protected override Task<bool> TickAsync() => Update();

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
					if(new [] {CardSet.LOOTAPALOOZA, CardSet.GILNEAS, CardSet.DALARAN, CardSet.ULDUM}.Contains(card.Set) && card.Id.Contains("BOSS") || card.Set == CardSet.TROLL && card.Id.EndsWith("h"))
					{
						if(card.Set == CardSet.DALARAN)
						{
							UpdateDungeonInfo();
							await Task.Delay(500).ConfigureAwait(false);
						}
						var newRun = _initialOpponents.Contains(_dataProvider.OpponentHeroId)
									|| _dataProvider.OpponentHeroHealth == 10;
						Dispatch(() => DungeonRunMatchStarted?.Invoke(newRun, card.Set));
						return true;
					}
				}
			}
			return false;
		}

		// deliberately unlocked, DeckManager calls this from the (posted) DungeonRunMatchStarted
		// handler and relies on DungeonInfoChanged firing inline before it recurses
		public bool UpdateDungeonInfo()
		{
			var dungeonInfo = Reflection.Client.GetDungeonInfo();
			if(dungeonInfo != null)
			{
				for(var i = 0; i < dungeonInfo.Length; i++)
				{
					if(dungeonInfo[i] != null && (dungeonInfo[i].RunActive || dungeonInfo[i].SelectedDeckId != 0))
					{
						if(_prevCards[i] == null || _prevCards[i]!.Count != (dungeonInfo[i].DbfIds?.Count ?? 0)
							|| _prevLootChoice[i] != dungeonInfo[i].PlayerChosenLoot
							|| _prevTreasureChoice[i] != dungeonInfo[i].PlayerChosenTreasure)
						{
							_prevCards[i] = dungeonInfo[i].DbfIds?.ToList() ?? new List<int>();
							_prevLootChoice[i] = dungeonInfo[i].PlayerChosenLoot;
							_prevTreasureChoice[i] = dungeonInfo[i].PlayerChosenTreasure;
							var info = dungeonInfo[i];
							Dispatch(() => DungeonInfoChanged?.Invoke(info));
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
				_prevCards = new List<int>?[] { null, null, null, null, null, null, null };
			}
			return false;
		}
	}
}
