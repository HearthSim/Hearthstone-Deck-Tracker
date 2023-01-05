using Newtonsoft.Json;
using System;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{
	public class EnumJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => objectType.IsEnum;

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
			JsonSerializer serializer) => throw new NotImplementedException();

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if(value == null)
				return;

			if(value is Array array)
			{
				writer.WriteStartArray();

				foreach(var item in array)
					if(Helper.TryGetAttribute<JsonPropertyAttribute>(item, out var attr) && attr != null)
						writer.WriteValue(attr.PropertyName);

				writer.WriteEndArray();
			} else if(Helper.TryGetAttribute<JsonPropertyAttribute>(value, out var attr) && attr != null)
					writer.WriteValue(attr.PropertyName);
		}
	}
}
