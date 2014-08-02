using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class CardIds
	{
		public static readonly List<string> InvalidCardIds = new List<string>
			{
				"EX1_tk34",
				"EX1_tk29",
				"EX1_tk28",
				"EX1_tk11",
				"EX1_598",
				"NEW1_032",
				"NEW1_033",
				"NEW1_034",
				"NEW1_009",
				"CS2_052",
				"CS2_082",
				"CS2_051",
				"CS2_050",
				"CS2_152",
				"skele11",
				"skele21",
				"GAME",
				"DREAM",
				"NEW1_006",
				"NAX"
			};

		public static readonly List<string> SecretIdsHunter = new List<string>
			{
				"EX1_610", //explosive trap
				"EX1_611", //freezing trap
				"EX1_533", //misdirection
				"EX1_609", //snipe
				"EX1_554" //snake trap
			};

		public static readonly List<string> SecretIdsMage = new List<string>
			{
				"EX1_287", //counterspell
				"FP1_018", //duplicate
				"EX1_289", //ice barrier
				"EX1_295", //ice block
				"EX1_294", //mirror entity
				"tt_010", //spellbender
				"EX1_594" //vaporize
			};

		public static readonly List<string> SecretIdsPaladin = new List<string>
			{
				"FP1_020", //avenge
				"EX1_132", //eye for an eye
				"EX1_130", //noble sacrifice
				"EX1_136", //redemption
				"EX1_379" //repentance
			};

		public static readonly Dictionary<string, string[]> SubCardIds = new Dictionary<string, string[]>
			{
				{
					//Ysera
					"EX1_572", new[]
						{
							"DREAM_01",
							"DREAM_02",
							"DREAM_03",
							"DREAM_04",
							"DREAM_05"
						}
				},
				{
					//ETC
					"PRO_001", new[]
						{
							"PRO_001a",
							"PRO_001b",
							"PRO_001c"
						}
				},
				{
					//Gelbin Mekkatorque
					"EX1_112", new[]
						{
							"Mekka1", 
							"Mekka2", 
							"Mekka3", 
							"Mekka4"
						}
				},
				{
					//Animal Companion
					"NEW1_031", new[]
						{
							"NEW1_032", 
							"NEW1_033", 
							"NEW1_034"
						}
				},
				{
					//Bane of Doom
					"EX1_320", new []
						{
							"EX1_306",
							"CS2_065",
							"EX1_319",
							"EX1_301",
							"CS2_059",
							"CS2_064"
						}
				},
				{
					//Power of the Horde
					"PRO_001c", new []
						{
							"CS2_121",
							"EX1_021",
							"EX1_023",
							"EX1_110",
							"EX1_390",
							"CS2_179"
						}
				}

			};
	}
}