#region

using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ConstructedDuelsMatchup: ConstructedMatchup
	{

		public ConstructedDuelsMatchup(HeroClassNeutral player, IEnumerable<GameStats> games) : base(player, games)
		{
			_games = games.Where(x => {
				var playerHeroClasses = GetPlayerHeroClasses(x);
				return playerHeroClasses != null && playerHeroClasses.Contains(player.ToString());
			});
		}

		public ConstructedDuelsMatchup(IEnumerable<GameStats> games) : base(games)
		{
			_games = games.Select(g =>
				{
					var games = new List<GameStats>();
					var playerHeroClasses = GetPlayerHeroClasses(g);
					if(playerHeroClasses != null)
						foreach(var playerHeroClass in playerHeroClasses)
						{
							var game = new GameStats(g.Result, g.OpponentHero, playerHeroClass);
							game.OpponentHeroClasses = g.OpponentHeroClasses;
							games.Add(game);
						}
					else
						games.Add(g);
					return games;
				}).SelectMany(g => g);
		}

		public MatchupStats Neutral => GetMatchupStats(HeroClassNeutral.Neutral);
		public override MatchupStats Total => new MatchupStats(
			"Total",
			_games.Select(g =>
			{
				var games = new List<GameStats>();
				var opponentHeroClasses = GetOpponentHeroClasses(g);
				if(opponentHeroClasses != null)
					foreach(var opponentHeroClass in opponentHeroClasses)
					{
						var game = new GameStats(g.Result, opponentHeroClass, g.PlayerHero);
						game.PlayerHeroClasses = g.PlayerHeroClasses;
						games.Add(game);
					}
				else
					games.Add(g);
				return games;
			}).SelectMany(g => g)
		);

		protected override MatchupStats GetMatchupStats(HeroClassNeutral opponent)
			=> new MatchupStats(
				opponent.ToString(),
				_games.Where(x => {
					var opponentHeroClasses = GetOpponentHeroClasses(x);
					return opponentHeroClasses != null && opponentHeroClasses.Contains(opponent.ToString());
				}).Select(x => x)
			);

		private string[]? GetPlayerHeroClasses(GameStats game) => game.PlayerHeroClasses != null
			? game.PlayerHeroClasses
			: game.PlayerHero != null && CardIds.DuelsHeroNameClass.TryGetValue(game.PlayerHero, out var playerHeroClasses)
				? playerHeroClasses : null;

		private string[]? GetOpponentHeroClasses(GameStats game) => game.OpponentHeroClasses != null
			? game.OpponentHeroClasses
			: game.OpponentHero != null && CardIds.DuelsHeroNameClass.TryGetValue(game.OpponentHero, out var opponentHeroClasses)
				? opponentHeroClasses : null;
	}
}
