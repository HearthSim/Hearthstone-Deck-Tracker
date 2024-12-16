using System.Collections.Generic;
using HearthDb.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Inspiration;

	public record InspirationApiResponse
	{
		[JsonProperty("data")]
		public ResponseData Data { get; init; } = new();

		public record ResponseData
		{
			[JsonProperty("lineups")]
			public Game[] Games { get; init; } = {};

			public record Game
			{
				[JsonProperty("hero_dbf_id")]
				public int HeroDbfId { get; init; }

				[JsonProperty("starting_hero_power")]
				public int HeroPower { get; init; }

				[JsonProperty("final_lineup")]
				public Minion[] FinalMinions { get; init; } = { };

				public record Minion
				{
					[JsonProperty("minion_dbf_id")]
					public int MinionDbfId { get; init; }

					[JsonProperty("zone_position")]
					public object ZonePosition { get; init; } = -1;

					[JsonProperty("attack")]
					public int Attack { get; init; }

					[JsonProperty("health")]
					public int Health { get; init; }

					[JsonProperty("premium")]
					public bool Premium { get; init; }

					[JsonProperty("divine_shield")]
					public bool DivineShield { get; init; }

					[JsonProperty("taunt")]
					public bool Taunt { get; init; }

					[JsonProperty("venemous")]
					public bool Venomous { get; init; }

					[JsonProperty("poison")]
					public bool Poisonous { get; init; }

					[JsonProperty("reborn")]
					public bool Reborn { get; init; }

					[JsonProperty("windfury")]
					public bool Windfury { get; init; }

					[JsonProperty("deathrattle")]
					public bool Deathrattle { get; init; }
				}
			}
		}
	}

	public record Trinket
	{
		[JsonProperty("trinket_dbf_id")]
		public int TrinketDbfId { get; init; }

		[JsonProperty("extra_data")]
		public int? ExtraData { get; init; }
	}

	public record InspirationApiRequestData(
		[property: JsonProperty("minion_types")]IEnumerable<Race> MinionTypes,
		[property: JsonProperty("key_card_dbf_ids")] IEnumerable<int> KeyCardDbfIds)
	{
		[JsonProperty("game_type")]
		public int GameType { get; init; } = (int)BnetGameType.BGT_UNKNOWN;

		[JsonProperty("lineup_dbf_ids")]
		public IEnumerable<int> LineupDbfIds { get; init; } = new int[] { };
	}
