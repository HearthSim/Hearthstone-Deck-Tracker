using Newtonsoft.Json;
using System;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{
	public class VMEnabledSettingsJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => true;

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
			JsonSerializer serializer) => throw new NotImplementedException();

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if(value == null)
				return;

			writer.WriteStartArray();
			foreach(var prop in value.GetType().GetProperties().Where(p => p.PropertyType == typeof(bool)))
			{
				var propJsonPropertyAttribute = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false)
					.FirstOrDefault();
				var propValue = (bool)prop.GetValue(value);

				if(propJsonPropertyAttribute != null && propValue)
					writer.WriteValue(((JsonPropertyAttribute)propJsonPropertyAttribute).PropertyName);
			}
			writer.WriteEndArray();
		}
	}
}
