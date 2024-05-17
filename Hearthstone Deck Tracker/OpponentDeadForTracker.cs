using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker
{
	public static class OpponentDeadForTracker
	{
		private static List<int> _uniqueDeadPlayers = new List<int>();
		private static List<int> _deadTracker = new List<int>();

		public static void ShoppingStarted(IGame game)
		{
			if(game.GetTurnNumber() <= 1)
				Reset();
			for(int i = 0; i < _deadTracker.Count; i++)
				_deadTracker[i]++;
			var deadHeroes = game.Entities.Values.Where(x => x.IsHero && x.Health <= 0);
			foreach(var hero in deadHeroes)
			{
				var playerId = hero.GetTag(GameTag.PLAYER_ID);
				if(playerId > 0 && !_uniqueDeadPlayers.Contains(playerId))
				{
					_deadTracker.Add(0);
					_uniqueDeadPlayers.Add(playerId);
				}
			}
			_deadTracker.Sort((x, y) => y.CompareTo(x));
			Core.Overlay.UpdateOpponentDeadForTurns(_deadTracker);
		}

		public static void SetNextOpponentPlayerId(int playerId, IGame game)
		{
			var nextOpponent = game.Entities.Values.FirstOrDefault(x => x.GetTag(GameTag.PLAYER_ID) == playerId);
			if(nextOpponent == null)
				return;

			var leaderboardPlace = nextOpponent.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE);
			if(leaderboardPlace >= 0 && leaderboardPlace < 8)
			{
				Core.Overlay.PositionDeadForText(leaderboardPlace);
			}
		}

		public static void Reset()
		{
			_uniqueDeadPlayers.Clear();
			_deadTracker.Clear();
			Core.Overlay.UpdateOpponentDeadForTurns(_deadTracker); // hide
		}
	}
}
