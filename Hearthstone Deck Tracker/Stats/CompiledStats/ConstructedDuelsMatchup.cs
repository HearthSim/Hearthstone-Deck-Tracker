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
			_games = games.Where(x =>
				x.PlayerHero != null &&
				CardIds.DuelsHeroNameClass.TryGetValue(x.PlayerHero, out var playerHeroClassNeutrals) &&
				playerHeroClassNeutrals.Contains(player.ToString())
			);
		}

		public ConstructedDuelsMatchup(IEnumerable<GameStats> games) : base(games)
		{
			_games = games.Select(g =>
				{
					var games = new List<GameStats>();
					if(g.PlayerHero != null && g.OpponentHero != null)
					{
						CardIds.DuelsHeroNameClass.TryGetValue(g.PlayerHero, out var playerHeroClassNeutrals);
						foreach(var playerHeroClassNeutral in playerHeroClassNeutrals)
							games.Add(new GameStats(g.Result, g.OpponentHero, playerHeroClassNeutral));
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
				if(g.PlayerHero != null && g.OpponentHero != null)
				{
					CardIds.DuelsHeroNameClass.TryGetValue(g.OpponentHero, out var opponentHeroClassNeutrals);
					if(opponentHeroClassNeutrals != null)
						foreach(var opponentHeroClassNeutral in opponentHeroClassNeutrals)
							games.Add(new GameStats(g.Result, opponentHeroClassNeutral, g.PlayerHero));
					else
						games.Add(g);
				}
				else
					games.Add(g);
				return games;
			}).SelectMany(g => g)
		);

		protected override MatchupStats GetMatchupStats(HeroClassNeutral opponent)
			=> new MatchupStats(
				opponent.ToString(),
				_games.Where(x =>
					x.OpponentHero != null &&
					CardIds.DuelsHeroNameClass.TryGetValue(x.OpponentHero, out var opponentHeroClassNeutrals) &&
					opponentHeroClassNeutrals.Contains(opponent.ToString())
				).Select(x => x)
			);
	}
}
