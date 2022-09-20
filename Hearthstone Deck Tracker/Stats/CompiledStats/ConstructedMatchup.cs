#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ConstructedMatchup
	{
		private readonly IEnumerable<GameStats> _games;
		private readonly HeroClassNeutral? _player;
		private readonly bool _isDuels;

		public ConstructedMatchup(HeroClassNeutral player, IEnumerable<GameStats> games, bool isDuels)
		{
			_player = player;
			_games = games.Where(x => isDuels
				? x.PlayerHero != null &&
					CardIds.DuelsHeroNameClass.TryGetValue(x.PlayerHero, out var playerHeroClassNeutrals) &&
					playerHeroClassNeutrals.Contains(player.ToString())
				: x.PlayerHero == player.ToString()
			);
			_isDuels = isDuels;
		}

		public ConstructedMatchup(IEnumerable<GameStats> games, bool isDuels)
		{
			_games = isDuels
				? games.Select(g =>
				{
					var games = new List<GameStats>();
					if(g.PlayerHero != null && g.OpponentHero != null)
					{
						CardIds.DuelsHeroNameClass.TryGetValue(g.PlayerHero, out var playerHeroClassNeutrals);
						CardIds.DuelsHeroNameClass.TryGetValue(g.OpponentHero, out var opponentHeroClassNeutrals);
						foreach(var playerHeroClassNeutral in playerHeroClassNeutrals)
							foreach(var opponentHeroClassNeutral in opponentHeroClassNeutrals)
							games.Add(new GameStats(g.Result, opponentHeroClassNeutral, playerHeroClassNeutral));
					}
					else
						games.Add(g);
					return games;
				}).SelectMany(g => g)
				: games;
		}

		public string Class => _player?.ToString() ?? "Total";
		public BitmapImage ClassImage => _player != null ? ImageCache.GetClassIcon(_player.ToString()) : new BitmapImage();
		public Visibility TextVisibility => _player != null ? Visibility.Collapsed : Visibility.Visible;
		public string Text => Class.ToUpper();

		public MatchupStats DemonHunter => GetMatchupStats(HeroClassNeutral.DemonHunter);
		public MatchupStats Druid => GetMatchupStats(HeroClassNeutral.Druid);
		public MatchupStats Hunter => GetMatchupStats(HeroClassNeutral.Hunter);
		public MatchupStats Mage => GetMatchupStats(HeroClassNeutral.Mage);
		public MatchupStats Paladin => GetMatchupStats(HeroClassNeutral.Paladin);
		public MatchupStats Priest => GetMatchupStats(HeroClassNeutral.Priest);
		public MatchupStats Rogue => GetMatchupStats(HeroClassNeutral.Rogue);
		public MatchupStats Shaman => GetMatchupStats(HeroClassNeutral.Shaman);
		public MatchupStats Warlock => GetMatchupStats(HeroClassNeutral.Warlock);
		public MatchupStats Warrior => GetMatchupStats(HeroClassNeutral.Warrior);
		public MatchupStats Neutral => GetMatchupStats(HeroClassNeutral.Neutral);
		public MatchupStats Total => new MatchupStats(
			"Total",
			_isDuels
				? _games.Select(g =>
				{
					var games = new List<GameStats>();
					if(g.PlayerHero != null && g.OpponentHero != null)
					{
						CardIds.DuelsHeroNameClass.TryGetValue(g.OpponentHero, out var opponentHeroClassNeutrals);
						foreach(var opponentHeroClassNeutral in opponentHeroClassNeutrals)
							games.Add(new GameStats(g.Result, opponentHeroClassNeutral, g.PlayerHero));
					}
					else
						games.Add(g);
					return games;
				}).SelectMany(g => g)
				: _games
		);

		private MatchupStats GetMatchupStats(HeroClassNeutral opponent)
			=> new MatchupStats(
				opponent.ToString(),
				_games.Where(x => _isDuels
					? x.OpponentHero != null &&
						CardIds.DuelsHeroNameClass.TryGetValue(x.OpponentHero, out var opponentHeroClassNeutrals) &&
						opponentHeroClassNeutrals.Contains(opponent.ToString())
					: x.OpponentHero == opponent.ToString()
				).Select(x => x)
			);
	}
}
