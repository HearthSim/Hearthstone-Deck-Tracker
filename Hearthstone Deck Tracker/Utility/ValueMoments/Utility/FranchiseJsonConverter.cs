using Newtonsoft.Json;
using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{
	public class FranchiseJsonConverter : JsonConverter<Franchise?>
	{
		public override Franchise? ReadJson(JsonReader reader, Type objectType, Franchise? existingValue, bool hasExistingValue,
			JsonSerializer serializer) =>
			throw new NotImplementedException();

		public override void WriteJson(JsonWriter writer, Franchise? value, JsonSerializer serializer)
		{
			if(value == null)
				return;

			writer.WriteStartArray();
			if(value == Franchise.All)
			{
				foreach(var franchise in Enum.GetValues(typeof(Franchise)).Cast<Franchise>().Where(f => f != Franchise.All))
					if(Helper.TryGetAttribute<JsonPropertyAttribute>(franchise, out var attr) && attr != null)
						writer.WriteValue(attr.PropertyName);
			}
			else if(Helper.TryGetAttribute<JsonPropertyAttribute>(value, out var attr) && attr != null)
				writer.WriteValue(attr.PropertyName);

			writer.WriteEndArray();
		}
	}
}
