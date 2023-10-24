using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Utility.RemoteData
{
	internal partial class RemoteData
	{
		public class LiveSecrets
		{
			[JsonProperty("by_game_type_and_format_type")]
			public Dictionary<string, HashSet<string>> ByType { get; set; } = new();
		}
	}
}
