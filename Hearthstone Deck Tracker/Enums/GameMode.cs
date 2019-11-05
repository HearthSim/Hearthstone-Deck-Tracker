namespace Hearthstone_Deck_Tracker.Enums
{
	public enum GameMode
	{
		[LocDescription("Enum_GameMode_All")]
		All, //for filtering @ deck stats
		[LocDescription("Enum_GameMode_Ranked")]
		Ranked,
		[LocDescription("Enum_GameMode_Casual")]
		Casual,
		[LocDescription("Enum_GameMode_Arena")]
		Arena,
		[LocDescription("Enum_GameMode_Brawl")]
		Brawl,
		[LocDescription("Enum_GameMode_Battlegrounds")]
		Battlegrounds,
		[LocDescription("Enum_GameMode_Friendly")]
		Friendly,
		[LocDescription("Enum_GameMode_Practice")]
		Practice,
		[LocDescription("Enum_GameMode_Spectator")]
		Spectator,
		[LocDescription("Enum_GameMode_None")]
		None
	}
}
