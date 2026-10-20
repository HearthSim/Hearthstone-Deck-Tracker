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

		public static bool operator ==(CardRef left, CardRef right) => left.Equals(right);

		public static bool operator !=(CardRef left, CardRef right) => !left.Equals(right);
	}

	/// <summary>
	/// A single board slot: the card itself plus any enchantments on it. Serializes as a bare card
	/// when there are no enchantments, otherwise as an array with the card first. A card with extra
	/// data, e.g. an assembled minion with the cards its text was assembled from, serializes as an
	/// object instead: {"card": ..., "enchantments": [...], "assembled_first_half": ..., ...}. The
	/// extension ignores object fields it does not know, so more can be added later.
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
			: this(new[] { card }.Concat(enchantments).ToArray(), null, null)
		{
		}

		private CardWithEnchantments(CardRef[] cards, CardRef? firstHalf, CardRef? secondHalf)
		{
			_cards = cards;
			FirstHalf = firstHalf;
			SecondHalf = secondHalf;
		}

		/// <summary>
		/// The card the first half of the text comes from, if it is assembled.
		/// </summary>
		public CardRef? FirstHalf { get; }

		/// <summary>
		/// The card the second half of the text comes from, if it is assembled.
		/// </summary>
		public CardRef? SecondHalf { get; }

		internal bool HasExtraData => FirstHalf.HasValue || SecondHalf.HasValue;

		public CardWithEnchantments WithHalves(CardRef? firstHalf, CardRef? secondHalf) => new(Cards, firstHalf, secondHalf);

		public static implicit operator CardWithEnchantments(int dbfId) => new CardWithEnchantments(dbfId);

		public static implicit operator CardWithEnchantments(CardRef card) => new CardWithEnchantments(card);

		internal CardRef[] Cards => _cards ?? Array.Empty<CardRef>();

		public bool Equals(CardWithEnchantments other)
			=> Cards.SequenceEqual(other.Cards) && FirstHalf == other.FirstHalf && SecondHalf == other.SecondHalf;

		public override bool Equals(object? obj) => obj is CardWithEnchantments other && Equals(other);

		public static bool operator ==(CardWithEnchantments left, CardWithEnchantments right) => left.Equals(right);

		public static bool operator !=(CardWithEnchantments left, CardWithEnchantments right) => !left.Equals(right);

		public override int GetHashCode()
			=> (Cards.Aggregate(17, (hash, card) => hash * 31 + card.GetHashCode()) * 31 + FirstHalf.GetHashCode()) * 31
				+ SecondHalf.GetHashCode();
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
			var slot = (CardWithEnchantments)value!;
			var cards = slot.Cards;
			if(slot.HasExtraData)
			{
				WriteObject(writer, slot, serializer);
				return;
			}
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

		// older extension versions do not understand the object, so only cards with extra data use it
		private static void WriteObject(JsonWriter writer, CardWithEnchantments slot, JsonSerializer serializer)
		{
			var cards = slot.Cards;
			writer.WriteStartObject();
			writer.WritePropertyName("card");
			serializer.Serialize(writer, cards[0]);
			if(cards.Length > 1)
			{
				writer.WritePropertyName("enchantments");
				writer.WriteStartArray();
				foreach(var card in cards.Skip(1))
					serializer.Serialize(writer, card);
				writer.WriteEndArray();
			}
			if(slot.FirstHalf.HasValue)
			{
				writer.WritePropertyName("assembled_first_half");
				serializer.Serialize(writer, slot.FirstHalf.Value);
			}
			if(slot.SecondHalf.HasValue)
			{
				writer.WritePropertyName("assembled_second_half");
				serializer.Serialize(writer, slot.SecondHalf.Value);
			}
			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var token = JToken.Load(reader);
			if(token is JObject obj)
			{
				var card = obj["card"] ?? throw new JsonSerializationException("Expected a card");
				var enchantments = obj["enchantments"]?.Select(x => x.ToObject<CardRef>(serializer)) ?? Enumerable.Empty<CardRef>();
				return new CardWithEnchantments(card.ToObject<CardRef>(serializer), enchantments)
					.WithHalves(obj["assembled_first_half"]?.ToObject<CardRef>(serializer), obj["assembled_second_half"]?.ToObject<CardRef>(serializer));
			}
			if(token.Type != JTokenType.Array)
				return new CardWithEnchantments(token.ToObject<CardRef>(serializer));
			var cards = token.Select(x => x.ToObject<CardRef>(serializer)).ToArray();
			if(cards.Length == 0)
				throw new JsonSerializationException("Expected at least one card");
			return new CardWithEnchantments(cards[0], cards.Skip(1));
		}
	}
}
