using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public record ArenaHeroPickApiResponse
{
	[JsonProperty("data")]
	public ResponseData[] Data { get; init; } = {};

	[JsonProperty("debug")]
	public object? Debug { get; init; }

	public record ResponseData
	{
		[JsonProperty("deck_class")]
		public int DeckClass { get; init; }

		[JsonProperty("tier")]
		public string? Tier { get; init; }

		[JsonProperty("abandon_rate")]
		public float AbandonRate { get; init; }

		[JsonProperty("pick_rate")]
		public float PickRate { get; init; }

		[JsonProperty("num_drafts")]
		public int NumDrafts { get; init; }

		[JsonProperty("popularity")]
		public float Popularity { get; init; }

		[JsonProperty("win_rate")]
		public float Winrate { get; init; }

		[JsonProperty("latest_winning_deckstrings")]
		public string[] LatestWinningDeckStrings { get; init; } = { };

		[JsonProperty("class_deck_signature")]
		public ClassDeckSignature ClassDeckSignature { get; init; } = new();
	}
}

public record ArenaCardPickApiResponse
{
	[JsonProperty("data")]
	public Dictionary<string, CardStatsEntry> Data { get; init; } = new();

	public record CardStatsEntry
	{
		[JsonProperty("arenasmith")]
		public ArenasmithScore? Arenasmith { get; init; } = new();

		[JsonProperty("arenasmith_dyn")]
		public ArenasmithDynScore? ArenasmithDyn { get; init; } = new();

		[JsonProperty("related_cards")]
		public RelatedCardsBlock RelatedCards { get; init; } = new();

		[JsonProperty("messages")]
		public List<Message> Messages { get; init; } = new();

		[JsonProperty("messages_old")]
		public Dictionary<string, List<string>>? MessagesOld { get; init; }
	}

	public record ArenasmithScore
	{
		[JsonProperty("score")]
		public string? Score { get; init; }

		[JsonProperty("plaque")]
		public int Plaque { get; init; }
	}

	public record ArenasmithDynScore
	{
		[JsonProperty("score")]
		public string? Score { get; init; }

		[JsonProperty("plaque")]
		public int Plaque { get; init; }

		[JsonProperty("caution")]
		public string? Caution { get; init; }
	}

	public record RelatedCardsBlock
	{
		[JsonProperty("generated_card_ids")]
		public GeneratedCardIdsBlock? GeneratedCardIds { get; init; }

		[JsonProperty("enhanced_by_card_ids")]
		public RelationSetBlock? EnhancedByCardIds { get; init; }

		[JsonProperty("card_ids_enabled")]
		public RelationSetBlock? CardIdsEnabled { get; init; }
	}

	public record GeneratedCardIdsBlock
	{
		[JsonProperty("generated")]
		public List<string> Generated { get; init; } = new();

		[JsonProperty("total_cards")]
		public int TotalCards { get; init; }

		[JsonProperty("is_sorted")]
		public bool IsSorted { get; init; }
	}

	public record RelationSetBlock
	{
		[JsonProperty("direct")]
		public List<string> Direct { get; init; } = new();

		[JsonProperty("indirect")]
		public List<string> Indirect { get; init; } = new();

		[JsonProperty("total_cards")]
		public int TotalCards { get; init; }
	}

	public record Message
	{
		[JsonProperty("type")]
		public MessageType Type { get; init; }

		[JsonProperty("content")]
		public Dictionary<string, object>? ContentRaw  { get; init; } = new();

		public T ParseContent<T>() where T : new()
		{
			if (ContentRaw == null || ContentRaw.Count == 0)
				return new T();

			var json = JsonConvert.SerializeObject(ContentRaw);
			return JsonConvert.DeserializeObject<T>(json);
		}

	}

}


public record ClassDeckSignature
{
	[JsonProperty("data")]
	public Dictionary<string, DeckSignatureEntry> Data { get; init; } = new();

	[JsonProperty("header")]
	public string Header { get; init; } = string.Empty;
}

public record DeckSignatureEntry
{
	[JsonProperty("12+ PPC")]
	public double Ppc { get; init; }

	[JsonProperty("Arenasmith")]
	public double? Arenasmith { get; init; }
}

public record ArenaCardStats
{
	[JsonProperty("data")]
	public Dictionary<string, ArenaCardStatsEntry> Data { get; init; } = new();
}

public record ArenaCardStatsEntry
{
	[JsonProperty("score")]
	public double? Score { get; init; }
}

public class EnhancedByMessageContent
{
	[JsonProperty("predicates")]
	public List<string> Predicates { get; set; } = new();

	[JsonProperty("ideal_num_enhancers")]
	public int? IdealNumEnhancers { get; set; }

	[JsonProperty("deck_count")]
	public int? DeckCount { get; set; }

	[JsonProperty("deck_count_generates")]
	public int? DeckCountGenerates { get; set; }

	[JsonProperty("pool_count")]
	public int? PoolCount { get; set; }

	[JsonProperty("pool_count_generates")]
	public int? PoolCountGenerates { get; set; }

	[JsonProperty("odds_remaining_picks")]
	public int? OddsRemainingPicks { get; set; }

	[JsonProperty("odds_1")]
	public int? Odds1 { get; set; }

	[JsonProperty("odds_2")]
	public int? Odds2 { get; set; }

	[JsonProperty("odds_3")]
	public int? Odds3 { get; set; }
}

public class LowSynergyMessageContent
{
	[JsonProperty("remaining_picks")]
	public int? RemainingPicks { get; set; }
}

public class HighlanderMessageContent
{

	[JsonProperty("highlander_card_id")]
	public string? HighlanderCardId { get; set; }
}

public class SoftHighlanderMessageContent
{
	[JsonProperty("highlander_card_id")]
	public string? HighlanderCardId { get; set; }
}

public class HighlanderChancesMessageContent
{
}

public class VeryRareMessageContent
{
	[JsonProperty("percent_drafts")]
	public double? PercentDrafts { get; set; }
}

public class QuestHelpsMessageContent
{
	[JsonProperty("quest_card_id")]
	public string? QuestCardId { get; set; }
}


[JsonConverter(typeof(MessageTypeConverter))]
public enum MessageType
{
	[EnumMember(Value = "INVALID")]
	Invalid,

	[EnumMember(Value = "ENHANCED_BY")]
	EnhancedBy,

	[EnumMember(Value = "CARDS_ENABLED")]
	CardsEnabled,

	[EnumMember(Value = "OFFERED_COPY")]
	OfferedCopy,

	[EnumMember(Value = "LOW_SYNERGY")]
	LowSynergy,

	[EnumMember(Value = "HIGHLANDER")]
	Highlander,

	[EnumMember(Value = "SOFT_HIGHLANDER")]
	SoftHighlander,

	[EnumMember(Value = "HIGHLANDER_CHANCES")]
	HighlanderChances,

	[EnumMember(Value = "VERY_RARE")]
	VeryRare,

	[EnumMember(Value = "SCORE_BOOST")]
	ScoreBoost,

	[EnumMember(Value = "QUEST_HELPS")]
	QuestHelps,

	[EnumMember(Value = "QUEST_NUM_REQ")]
	QuestNumReq
}

public sealed class MessageTypeConverter : JsonConverter<MessageType>
{
	public override MessageType ReadJson(
		JsonReader reader,
		Type objectType,
		MessageType existingValue,
		bool hasExistingValue,
		JsonSerializer serializer)
	{
		if (reader.TokenType != JsonToken.String)
			return MessageType.Invalid;

		var s = (reader.Value as string)?.Trim();
		if (string.IsNullOrEmpty(s))
			return MessageType.Invalid;

		s = s?.ToUpperInvariant();

		return s switch
		{
			"ENHANCED_BY"        => MessageType.EnhancedBy,
			"CARDS_ENABLED"      => MessageType.CardsEnabled,
			"OFFERED_COPY"       => MessageType.OfferedCopy,
			"LOW_SYNERGY"        => MessageType.LowSynergy,
			"HIGHLANDER"         => MessageType.Highlander,
			"SOFT_HIGHLANDER"    => MessageType.SoftHighlander,
			"HIGHLANDER_CHANCES" => MessageType.HighlanderChances,
			"VERY_RARE"          => MessageType.VeryRare,
			"SCORE_BOOST"        => MessageType.ScoreBoost,
			"QUEST_HELPS"        => MessageType.QuestHelps,
			"QUEST_NUM_REQ"      => MessageType.QuestNumReq,
			"INVALID"            => MessageType.Invalid,
			_                    => MessageType.Invalid
		};
	}

	public override void WriteJson(JsonWriter writer, MessageType value, JsonSerializer serializer)
	{
		var s = value switch
		{
			MessageType.EnhancedBy        => "ENHANCED_BY",
			MessageType.CardsEnabled      => "CARDS_ENABLED",
			MessageType.OfferedCopy       => "OFFERED_COPY",
			MessageType.LowSynergy        => "LOW_SYNERGY",
			MessageType.Highlander        => "HIGHLANDER",
			MessageType.SoftHighlander    => "SOFT_HIGHLANDER",
			MessageType.HighlanderChances => "HIGHLANDER_CHANCES",
			MessageType.VeryRare          => "VERY_RARE",
			MessageType.ScoreBoost        => "SCORE_BOOST",
			MessageType.QuestHelps        => "QUEST_HELPS",
			MessageType.QuestNumReq       => "QUEST_NUM_REQ",
			_                                  => "INVALID"
		};
		writer.WriteValue(s);
	}
}
