using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum ArenaRewardPacks
	{
		[LocDescription("Enum_ArenaRewardPacks_None")]
		None = CardSet.INVALID,

		[LocDescription("Enum_ArenaRewardPacks_Classic")]
		Classic = CardSet.EXPERT1,

		[LocDescription("Enum_ArenaRewardPacks_GoblinsVsGnomes")]
		GoblinsVsGnomes = CardSet.GVG,

		[LocDescription("Enum_ArenaRewardPacks_TheGrandTournament")]
		TheGrandTournament = CardSet.TGT,

		[LocDescription("Enum_ArenaRewardPacks_WhispersOfTheOldGods")]
		WhispersOfTheOldGods = CardSet.OG,

		[LocDescription("Enum_ArenaRewardPacks_MeanStreetsOfGadgetzan")]
		MeanStreetsOfGadgetzan = CardSet.GANGS,

		[LocDescription("Enum_ArenaRewardPacks_JourneyToUngoro")]
		JourneyToUngoro = CardSet.UNGORO,

		[LocDescription("Enum_ArenaRewardPacks_KnightsOfTheFrozenThrone")]
		KnightsOfTheFrozenThrone = CardSet.ICECROWN,

		[LocDescription("Enum_ArenaRewardPacks_KoboldsAndCatacombs")]
		Loot = CardSet.LOOTAPALOOZA,

		[LocDescription("Enum_ArenaRewardPacks_Witchwood")]
		Gilneas = CardSet.GILNEAS,

		[LocDescription("Enum_ArenaRewardPacks_Boomsday")]
		Boomsday = CardSet.BOOMSDAY,

		[LocDescription("Enum_ArenaRewardPacks_Troll")]
		Troll = CardSet.TROLL,

		[LocDescription("Enum_ArenaRewardPacks_Dalaran")]
		Dalaran = CardSet.DALARAN,

		[LocDescription("Enum_ArenaRewardPacks_Uldum")]
		Uldum = CardSet.ULDUM,

		[LocDescription("Enum_ArenaRewardPacks_Dragons")]
		Dragons = CardSet.DRAGONS,

		[LocDescription("Enum_ArenaRewardPacks_BlackTemple")]
		BlackTemple = CardSet.BLACK_TEMPLE,

		[LocDescription("Enum_ArenaRewardPacks_Scholomance")]
		Scholomance = CardSet.SCHOLOMANCE,

		[LocDescription("Enum_ArenaRewardPacks_DarkmoonFaire")]
		DarkmoonFaire = CardSet.DARKMOON_FAIRE,

		[LocDescription("Enum_ArenaRewardPacks_TheBarrens")]
		TheBarrens = CardSet.THE_BARRENS,

		[LocDescription("Enum_ArenaRewardPacks_Stormwind")]
		Stormwind = CardSet.STORMWIND,

		[LocDescription("Enum_ArenaRewardPacks_Alterac")]
		AlteracValley = CardSet.ALTERAC_VALLEY,

		[LocDescription("Enum_ArenaRewardPacks_SunkenCity")]
		SunkenCity = CardSet.THE_SUNKEN_CITY,

		[LocDescription("Enum_ArenaRewardPacks_Nathria")]
		Nathria = CardSet.REVENDRETH,
	}
}
