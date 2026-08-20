using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hearthstone_Deck_Tracker.Live.Data
{
	/// <summary>
	/// A card, identified either by its dbf id or (in future) by its card id. Serializes as a bare
	/// number or a bare string, never as an object.
	/// </summary>
	[JsonConverter(typeof(CardRefConverter))]
	public readonly struct CardRef : IEquatable<CardRef>
	{
		private readonly int _dbfId;
		private readonly string? _cardId;

		private CardRef(int dbfId, string? cardId)
		{
			_dbfId = dbfId;
			_cardId = cardId;
		}

		public static implicit operator CardRef(int dbfId) => new CardRef(dbfId, null);

		public static implicit operator CardRef(string cardId) => new CardRef(0, cardId);

		internal object Value => (object?)_cardId ?? _dbfId;

		public bool Equals(CardRef other) => _dbfId == other._dbfId && _cardId == other._cardId;

		public override bool Equals(object? obj) => obj is CardRef other && Equals(other);

		public override int GetHashCode() => _cardId?.GetHashCode() ?? _dbfId;
	}

	/// <summary>
	/// A single board slot: the card itself plus any enchantments on it. Serializes as a bare card
	/// when there are no enchantments, otherwise as an array with the card first.
	/// </summary>
	[JsonConverter(typeof(CardWithEnchantmentsConverter))]
	public readonly struct CardWithEnchantments : IEquatable<CardWithEnchantments>
	{
		private readonly CardRef[]? _cards;

		public CardWithEnchantments(CardRef card, params CardRef[] enchantments)
			: this(card, (IEnumerable<CardRef>)enchantments)
		{
		}

		public CardWithEnchantments(CardRef card, IEnumerable<CardRef> enchantments)
			=> _cards = new[] { card }.Concat(enchantments).ToArray();

		public static implicit operator CardWithEnchantments(int dbfId) => new CardWithEnchantments(dbfId);

		internal CardRef[] Cards => _cards ?? Array.Empty<CardRef>();

		public bool Equals(CardWithEnchantments other) => Cards.SequenceEqual(other.Cards);

		public override bool Equals(object? obj) => obj is CardWithEnchantments other && Equals(other);

		public override int GetHashCode() => Cards.Aggregate(17, (hash, card) => hash * 31 + card.GetHashCode());
	}

	public class CardRefConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => objectType == typeof(CardRef);

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
			=> writer.WriteValue(((CardRef)value!).Value);

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			switch(reader.TokenType)
			{
				case JsonToken.Integer:
					return (CardRef)Convert.ToInt32(reader.Value);
				case JsonToken.String:
					return (CardRef)(string)reader.Value!;
				default:
					throw new JsonSerializationException($"Unexpected token {reader.TokenType} while reading a card");
			}
		}
	}

	public class CardWithEnchantmentsConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => objectType == typeof(CardWithEnchantments);

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			var cards = ((CardWithEnchantments)value!).Cards;
			if(cards.Length == 1)
			{
				serializer.Serialize(writer, cards[0]);
				return;
			}
			writer.WriteStartArray();
			foreach(var card in cards)
				serializer.Serialize(writer, card);
			writer.WriteEndArray();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var token = JToken.Load(reader);
			if(token.Type != JTokenType.Array)
				return new CardWithEnchantments(token.ToObject<CardRef>(serializer));
			var cards = token.Select(x => x.ToObject<CardRef>(serializer)).ToArray();
			if(cards.Length == 0)
				throw new JsonSerializationException("Expected at least one card");
			return new CardWithEnchantments(cards[0], cards.Skip(1));
		}
	}
}
