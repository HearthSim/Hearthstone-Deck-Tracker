using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public class Franchise
	{
		public const string HSConstructedValue = "HS-Constructed";
		public const string BattlegroundsValue = "Battlegrounds";
		public const string MercenariesValue = "Mercenaries";

		public Franchise(string value)
		{
			Value = value;
		}

		public string Value { get; }

		public static Franchise HSConstructed = new(HSConstructedValue);
		public static Franchise Battlegrounds = new(BattlegroundsValue);
		public static Franchise Mercenaries = new(MercenariesValue);
	}
}
