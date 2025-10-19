using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Hearthstone_Deck_Tracker.Utility.RemoteData
{
	internal partial class RemoteData
	{
		public class LiveSecrets
		{
			[JsonProperty("by_game_type_and_format_type")]
			public Dictionary<string, HashSet<string>> ByType { get; set; } = new();

			[JsonProperty("created_by_game_type_and_format_type")]
			[JsonConverter(typeof(GameTypeFormatTypeConverter))]
			public Dictionary<string, Dictionary<string, HashSet<string>>>? CreatedByTypeByCreator { get; set; } = new();
		}
	}

	public class GameTypeFormatTypeConverter : JsonConverter<Dictionary<string, Dictionary<string, HashSet<string>>>?>
	{
		public override Dictionary<string, Dictionary<string, HashSet<string>>> ReadJson(JsonReader reader, Type objectType, Dictionary<string, Dictionary<string, HashSet<string>>>? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var result = new Dictionary<string, Dictionary<string, HashSet<string>>>();

			var token = JToken.Load(reader);

			if (token.Type != JTokenType.Object)
				return result;

			foreach (var property in ((JObject)token).Properties())
			{
				if (property.Value.Type == JTokenType.Object)
				{
					// Normal case: Dictionary<string, HashSet<string>>
					var innerDict = property.Value.ToObject<Dictionary<string, HashSet<string>>>(serializer);
					result[property.Name] = innerDict ?? new Dictionary<string, HashSet<string>>();
				}
				else if (property.Value.Type == JTokenType.Array)
				{
					// Empty array: []
					result[property.Name] = new Dictionary<string, HashSet<string>>();
				}
			}

			return result;
		}

		public override void WriteJson(JsonWriter writer, Dictionary<string, Dictionary<string, HashSet<string>>>? value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}
