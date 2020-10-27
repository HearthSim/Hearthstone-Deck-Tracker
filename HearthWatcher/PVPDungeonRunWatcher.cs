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
				if(_prevCards != null)
				{
					PVPDungeonRunMatchStarted.Invoke(_prevCards.Count == 16, CardSet.NONE);
				}
				if(shouldBreak)
					return true;
			}
			return false;
		}

		public bool UpdatePVPDungeonInfo()
		{
			var pvpDungeonInfo = Reflection.GetPVPDungeonInfo();
			if(pvpDungeonInfo != null)
			{
				if(pvpDungeonInfo.RunActive || pvpDungeonInfo.SelectedDeckId != 0)
				{
					if(_prevCards == null || _prevCards.Count != (pvpDungeonInfo.DbfIds?.Count ?? 0) || _prevLootChoice != pvpDungeonInfo.PlayerChosenLoot || _prevTreasureChoice != pvpDungeonInfo.PlayerChosenTreasure)
					{
						_prevCards = pvpDungeonInfo.DbfIds?.ToList() ?? new List<int>();
						_prevLootChoice = pvpDungeonInfo.PlayerChosenLoot;
						_prevTreasureChoice = pvpDungeonInfo.PlayerChosenTreasure;
						PVPDungeonInfoChanged?.Invoke(pvpDungeonInfo);
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
