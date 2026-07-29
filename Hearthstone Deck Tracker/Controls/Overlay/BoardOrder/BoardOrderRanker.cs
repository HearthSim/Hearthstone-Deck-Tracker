using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using HearthWatcher.EventArgs;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.BoardOrder
{
	public class BoardOrderRanker
	{
		// Reused across updates. Safe because every caller (log batches and watcher ticks alike)
		// runs on the UI thread: both loops are async void awaiting on the UI SynchronizationContext.
		private readonly List<(int Id, int Order)> _onScreen = new();
		private readonly Dictionary<int, int> _ranks = new();

		public IReadOnlyDictionary<int, int> Compute(
			GameV2 game,
			PlayZoneArgs? friendlyZone, Entity? friendlyWeapon,
			PlayZoneArgs? opposingZone, Entity? opposingWeapon)
		{
			_onScreen.Clear();
			Collect(game, friendlyZone, friendlyWeapon);
			Collect(game, opposingZone, opposingWeapon);

			_onScreen.Sort((a, b) => a.Order.CompareTo(b.Order));

			_ranks.Clear();
			for(var i = 0; i < _onScreen.Count; i++)
				_ranks[_onScreen[i].Id] = i + 1;
			return _ranks;
		}

		private void Collect(GameV2 game, PlayZoneArgs? zone, Entity? weapon)
		{
			var cards = zone?.BoardCards;
			if(cards != null)
			{
				foreach(var card in cards)
				{
					if(card?.EntityId is int id
						&& game.Entities.TryGetValue(id, out var entity)
						&& entity.Info.BoardOrder is int order)
						_onScreen.Add((id, order));
				}
			}

			// Weapons are not in the play zone but share the sequence with minions.
			if(weapon?.Info.BoardOrder is int weaponOrder)
				_onScreen.Add((weapon.Id, weaponOrder));
		}
	}
}
