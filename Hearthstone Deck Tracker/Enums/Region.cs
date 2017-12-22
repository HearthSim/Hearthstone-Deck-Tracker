namespace Hearthstone_Deck_Tracker.Enums
{
	public enum Region
	{
		[LocDescription("Enum_Region_Unknown")]
		UNKNOWN = 0,
		[LocDescription("Enum_Region_US")]
		US = 1,
		[LocDescription("Enum_Region_EU")]
		EU = 2,
		[LocDescription("Enum_Region_Asia")]
		ASIA = 3,
		[LocDescription("Enum_Region_China")]
		CHINA = 5
	}

	public enum RegionAll
	{
		[LocDescription("Enum_Region_All")]
		ALL = -1,
		[LocDescription("Enum_Region_Unknown")]
		UNKNOWN = 0,
		[LocDescription("Enum_Region_US")]
		US = 1,
		[LocDescription("Enum_Region_EU")]
		EU = 2,
		[LocDescription("Enum_Region_Asia")]
		ASIA = 3,
		[LocDescription("Enum_Region_China")]
		CHINA = 5
	}
}
