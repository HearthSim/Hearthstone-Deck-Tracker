using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker
{
	public static class OpponentDeadForTracker
	{
		private static List<string> _uniqueDeadHeroes = new List<string>();
		private static List<int> _deadTracker = new List<int>();
		const string KelThuzadCardId = "KelThuzad";

		public static void ShoppingStarted(GameV2 game)
		{
			if(game.GetTurnNumber() <= 1)
			{
				_uniqueDeadHeroes.Clear();
				_deadTracker.Clear();
				Core.Overlay.nextOpponentLeaderboardPosition = null;
			}
			for(int i =0; i < _deadTracker.Count; i++)
			{
				_deadTracker[i]++;
			}
			var deadHeroes = game.Entities.Select(x=>x.Value).Where(x => x.IsHero && x.Health <= 0);
			foreach(var hero in deadHeroes)
			{
				var id = BattlegroundsBoardState.GetCorrectBoardstateHeroId(hero.CardId);
				if(!id.Contains(KelThuzadCardId) && !_uniqueDeadHeroes.Contains(id))
				{
					_deadTracker.Add(0);
					_uniqueDeadHeroes.Add(id);
				}
			}
			_deadTracker.Sort((x, y) => y.CompareTo(x));
			Core.Overlay.UpdateOpponentDeadForTurns(_deadTracker);
			var gameEntites = game.Entities.Select(x => x.Value).ToList();
			var currentPlayer = gameEntites.FirstOrDefault(x => x.IsCurrentPlayer);
			if(currentPlayer != null && currentPlayer.HasTag(GameTag.NEXT_OPPONENT_PLAYER_ID))
			{
				var nextOpponent = gameEntites.FirstOrDefault(x => x.GetTag(GameTag.PLAYER_ID) == currentPlayer.GetTag(GameTag.NEXT_OPPONENT_PLAYER_ID));
				if(nextOpponent != null)
				{
					var leaderboardPlace = nextOpponent.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE);
					if(leaderboardPlace >= 0 && leaderboardPlace < 8)
					{
						Core.Overlay.nextOpponentLeaderboardPosition = leaderboardPlace;
						Core.Overlay.PositionDeadForText();
					}
				}
			}
		}
	}
}
