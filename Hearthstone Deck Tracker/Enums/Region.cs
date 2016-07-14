#region

using System.ComponentModel;

#endregion

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum Region
	{
		UNKNOWN = 0,
		US = 1,
		EU = 2,
		ASIA = 3,
		CHINA = 5
	}

	public enum RegionAll
	{
		[Description("All")]
		ALL = -1,

		[Description("Unknown")]
		UNKNOWN = 0,

		[Description("US")]
		US = 1,

		[Description("EU")]
		EU = 2,

		[Description("Asia")]
		ASIA = 3,

		[Description("China")]
		CHINA = 5
	}
}