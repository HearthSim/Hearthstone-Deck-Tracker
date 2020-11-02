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
	public class PVPDungeonRunWatcher
	{
		private readonly IGameDataProvider _dataProvider;
		private readonly int _delay;
		public bool Running { get; private set; }
		private bool _watch;
		private List<int> _prevCards;
		private int? _prevLootChoice;
		private int? _prevTreasureChoice;

		public event Action<DungeonInfo> PVPDungeonInfoChanged;
		public event Action<bool, CardSet> PVPDungeonRunMatchStarted;

		public PVPDungeonRunWatcher(IGameDataProvider dataProvider, int delay = 500)
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
			_prevLootChoice = null;
			_prevTreasureChoice = null;
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

		public bool Update()
		{
			if(_dataProvider.InPVPDungeonRunScreen)
			{
				var shouldBreak = UpdatePVPDungeonInfo();
				if(shouldBreak)
					return true;
			}
			else if(_dataProvider.InPVPDungeonRunMatch && !string.IsNullOrEmpty(_dataProvider.OpponentHeroId))
			{
				PVPDungeonRunMatchStarted.Invoke(false, CardSet.DARKMOON_FAIRE);
				return true;
			}
			return false;
		}

		public bool UpdatePVPDungeonInfo()
		{
			var pvpDungeonInfo = Reflection.GetPVPDungeonInfo();
			if(pvpDungeonInfo != null)
			{
				if(pvpDungeonInfo.RunActive)
				{
					if(_prevCards == null || _prevCards.Count != (pvpDungeonInfo.DbfIds?.Count ?? 0) || _prevLootChoice != pvpDungeonInfo.PlayerChosenLoot || _prevTreasureChoice != pvpDungeonInfo.PlayerChosenTreasure)
					{
						_prevCards = pvpDungeonInfo.DbfIds?.ToList() ?? new List<int>();
						_prevLootChoice = pvpDungeonInfo.PlayerChosenLoot;
						_prevTreasureChoice = pvpDungeonInfo.PlayerChosenTreasure;
						PVPDungeonInfoChanged?.Invoke(pvpDungeonInfo);
					}
				}
				else if(!string.IsNullOrEmpty(pvpDungeonInfo.LoadoutCardId))
				{
					var deck = Reflection.GetPVPDungeonSeedDeck();
					if(deck == null) {
						return false;
					}
					var dbfids = deck.Cards.Select(x => HearthDb.Cards.All.TryGetValue(x.Id, out var card) ? card.DbfId : -1).ToList();
					if(dbfids.Any(x => x == -1))
						return false;
					if(dbfids.Count == 15)
					{
						pvpDungeonInfo.UpdateDbfids(dbfids);
						PVPDungeonInfoChanged?.Invoke(pvpDungeonInfo);
						return true;
					}
				}
				else
				{
					_prevCards = null;
				}
				if(_prevLootChoice > 0 && _prevTreasureChoice > 0)
					return true;
			}
			else
			{
				_prevCards = null;
			}
			return false;
		}
	}
}
