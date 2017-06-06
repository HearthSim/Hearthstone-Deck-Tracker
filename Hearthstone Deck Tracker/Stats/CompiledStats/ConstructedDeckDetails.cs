using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ConstructedDeckDetails
	{
		public string Version { get; }
		private readonly IEnumerable<GameStats> _games;

		public ConstructedDeckDetails(string shortVersionString, IEnumerable<GameStats> games)
		{
			Version = shortVersionString;
			_games = games;
		}

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
