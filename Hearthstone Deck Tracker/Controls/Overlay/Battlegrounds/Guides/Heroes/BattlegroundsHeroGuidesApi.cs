using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Heroes;

public class HeroGuidesApiResponse : List<BattlegroundsHeroGuide>
{
}

public record BattlegroundsHeroGuide
{
	[JsonProperty("id")]
	public int Id { get; init; }

	[JsonProperty("hero")]
	public int Hero { get; init; }

	[JsonProperty("published_guide")]
	public string PublishedGuide { get; init; } = string.Empty;

	[JsonProperty("last_updated")]
	public DateTime? LastUpdated { get; init; }

	[JsonProperty("favorable_tribes")]
	public List<int>? FavorableTribes { get; init; } = new();

	[JsonProperty("hidden")]
	public bool Hidden { get; init; }

	[JsonProperty("published_guide_length")]
	public int PublishedGuideLength { get; init; }

	[JsonProperty("ready")]
	public bool Ready { get; init; }
}
