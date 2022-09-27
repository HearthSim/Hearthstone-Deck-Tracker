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
		protected IEnumerable<GameStats> _games;
		protected readonly HeroClassNeutral? _player;

		public ConstructedMatchup(HeroClassNeutral player, IEnumerable<GameStats> games)
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
		public virtual MatchupStats Total => new MatchupStats("Total", _games);

		protected virtual MatchupStats GetMatchupStats(HeroClassNeutral opponent)
			=> new MatchupStats(opponent.ToString(), _games.Where(x => x.OpponentHero == opponent.ToString()).Select(x => x));
	}
}
