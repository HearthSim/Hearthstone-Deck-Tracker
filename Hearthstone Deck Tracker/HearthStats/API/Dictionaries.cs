#region

using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	public class Dictionaries
	{
		public static readonly Dictionary<int, GameResult> GameResultDict = new Dictionary<int, GameResult>
		{
			{1, GameResult.Win},
			{2, GameResult.Loss},
			{3, GameResult.Draw}
		};

		public static readonly Dictionary<int, GameMode> GameModeDict = new Dictionary<int, GameMode>
		{
			{1, GameMode.Arena},
			{2, GameMode.Casual},
			{3, GameMode.Ranked},
			{4, GameMode.None}, //Tournament
			{5, GameMode.Friendly}
		};

		public static readonly Dictionary<int, string> HeroDict = new Dictionary<int, string>
		{
			{1, "Druid"},
			{2, "Hunter"},
			{3, "Mage"},
			{4, "Paladin"},
			{5, "Priest"},
			{6, "Rogue"},
			{7, "Shaman"},
			{8, "Warlock"},
			{9, "Warrior"}
		};
	}
}