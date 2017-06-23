#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ConstructedMatchup
	{
		private readonly IEnumerable<GameStats> _games;
		private readonly HeroClass? _player;

		public ConstructedMatchup(HeroClass player, IEnumerable<GameStats> games)
		{
			_player = player;
			_games = games.Where(x => x.PlayerHero == player.ToString());
		}

		public ConstructedMatchup(IEnumerable<GameStats> games)
		{
			_games = games;
		}

		public string Class => _player?.ToString() ?? "Total";
		public BitmapImage ClassImage => _player != null ? ImageCache.GetClassIcon(_player.ToString()) : new BitmapImage();
		public Visibility TextVisibility => _player != null ? Visibility.Collapsed : Visibility.Visible;
		public string Text => Class.ToUpper();

		public MatchupStats Druid => GetMatchupStats(HeroClass.Druid);
		public MatchupStats Hunter => GetMatchupStats(HeroClass.Hunter);
		public MatchupStats Mage => GetMatchupStats(HeroClass.Mage);
		public MatchupStats Paladin => GetMatchupStats(HeroClass.Paladin);
		public MatchupStats Priest => GetMatchupStats(HeroClass.Priest);
		public MatchupStats Rogue => GetMatchupStats(HeroClass.Rogue);
		public MatchupStats Shaman => GetMatchupStats(HeroClass.Shaman);
		public MatchupStats Warlock => GetMatchupStats(HeroClass.Warlock);
		public MatchupStats Warrior => GetMatchupStats(HeroClass.Warrior);
		public MatchupStats Total => new MatchupStats("Total", _games);

		public MatchupStats GetMatchupStats(HeroClass opponent)
			=> new MatchupStats(opponent.ToString(), _games.Where(x => x.OpponentHero == opponent.ToString()).Select(x => x));
	}
}
