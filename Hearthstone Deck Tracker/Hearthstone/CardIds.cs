#region

using System;
using System.Collections.Generic;

#endregion

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
			"NAX",
			"FP1_006",
			"PART",
			"BRMA",
			"BRMC",
			"TBA",
			"TB_",
			"TBST_"
		};

	    public static class Secrets
        {
            public static class Hunter
            {
                public static string[] All
                {
                    get { return new[] { BearTrap, ExplosiveTrap, FreezingTrap, Misdirection, Snipe, SnakeTrap }; }
                }

                public static string BearTrap { get { return "AT_060"; } }
                public static string ExplosiveTrap { get { return "EX1_610"; } }
                public static string FreezingTrap { get { return "EX1_611"; } }
                public static string Misdirection { get { return "EX1_533"; } }
                public static string Snipe { get { return "EX1_609"; } }
                public static string SnakeTrap { get { return "EX1_554"; } }
            }
            public static class Mage
            {
                public static string[] All
                {
                    get { return new[] { Counterspell, Duplicate, Effigy, IceBarrier, IceBlock, MirrorEntity, Spellbender, Vaporize }; }
                }
                public static string Counterspell { get { return "EX1_287"; } }
                public static string Duplicate { get { return "FP1_018"; } }
                public static string Effigy { get { return "AT_002"; } }
                public static string IceBarrier { get { return "EX1_289"; } }
                public static string IceBlock { get { return "EX1_295"; } }
                public static string MirrorEntity { get { return "EX1_294"; } }
                public static string Spellbender { get { return "tt_010"; } }
                public static string Vaporize { get { return "EX1_594"; } }
            }
            public static class Paladin
            {
                public static string[] All
                {
                    get { return new[] { Avenge, CompetitiveSpirit, EyeForAnEye, NobleSacrifice, Redemption, Repentance }; }
                }
                public static string Avenge { get { return "FP1_020"; } }
                public static string CompetitiveSpirit { get { return "AT_073"; } }
                public static string EyeForAnEye { get { return "EX1_132"; } }
                public static string NobleSacrifice { get { return "EX1_130"; } }
                public static string Redemption { get { return "EX1_136"; } }
                public static string Repentance { get { return "EX1_379"; } }
            }

            public static List<string> FastCombat = new List<string> {
                Hunter.FreezingTrap,
                Hunter.ExplosiveTrap,
                Hunter.Misdirection,
                Paladin.NobleSacrifice,
                Mage.Vaporize
            };
        }

        [Obsolete("Use Secrets.Hunter.All")]
        public static readonly List<string> SecretIdsHunter = new List<string>
		{
			"AT_060",  //bear trap
			"EX1_610", //explosive trap
			"EX1_611", //freezing trap
			"EX1_533", //misdirection
			"EX1_609", //snipe
			"EX1_554"  //snake trap
		};

        [Obsolete("Use Secrets.Mage.All")]
        public static readonly List<string> SecretIdsMage = new List<string>
		{
			"EX1_287", //counterspell
			"FP1_018", //duplicate
			"AT_002",  //effigy
            "EX1_289", //ice barrier
			"EX1_295", //ice block
			"EX1_294", //mirror entity
			"tt_010",  //spellbender
			"EX1_594"  //vaporize
		};

        [Obsolete("Use Secrets.Paladin.All")]
        public static readonly List<string> SecretIdsPaladin = new List<string>
		{
			"FP1_020", //avenge
			"AT_073",  //competitive spirit
            "EX1_132", //eye for an eye
			"EX1_130", //noble sacrifice
			"EX1_136", //redemption
			"EX1_379"  //repentance
		};

        public static readonly Dictionary<string, string[]> SubCardIds = new Dictionary<string, string[]>
		{
			{
				//Ysera
				"EX1_572", new[] {"DREAM_01", "DREAM_02", "DREAM_03", "DREAM_04", "DREAM_05"}
			},
			{
				//ETC
				"PRO_001", new[] {"PRO_001a", "PRO_001b", "PRO_001c"}
			},
			{
				//Gelbin Mekkatorque
				"EX1_112", new[] {"Mekka1", "Mekka2", "Mekka3", "Mekka4"}
			},
			{
				//Animal Companion
				"NEW1_031", new[] {"NEW1_032", "NEW1_033", "NEW1_034"}
			},
			{
				//Bane of Doom
				"EX1_320",
				new[]
				{
					"EX1_306",
					"CS2_065",
					"EX1_319",
					"EX1_301",
					"CS2_059",
					"CS2_064",
					"EX1_323",
					"GVG_021",
					"EX1_614",
					"EX1_310",
					"GVG_100",
					"EX1_313",
					"FP1_022",
					"EX1_304",
					"GVG_018",
					"BRM_006"
				}
			},
			{
				//Power of the Horde
				"PRO_001c", new[] {"CS2_121", "EX1_021", "EX1_023", "EX1_110", "EX1_390", "CS2_179"}
			},
			{
				//Dr. Boom
				"GVG_110", new[] {"GVG_110t"}
			}
		};

		public static readonly Dictionary<string, string> HeroIdDict = new Dictionary<string, string>
		{
			{"HERO_01", "Warrior"},
			{"HERO_02", "Shaman"},
			{"HERO_03", "Rogue"},
			{"HERO_04", "Paladin"},
			{"HERO_05", "Hunter"},
			{"HERO_06", "Druid"},
			{"HERO_07", "Warlock"},
			{"HERO_08", "Mage"},
			{"HERO_09", "Priest"},
			{"EX1_323", "Jaraxxus"},
			{"BRM_027", "Ragnaros the Firelord"}
		};
	}
}