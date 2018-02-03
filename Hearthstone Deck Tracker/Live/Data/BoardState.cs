using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Live.Data
{
	public class GameStart
	{
		[JsonProperty("deck")]
		public BoardStateDeck Deck { get; set; }

		[JsonProperty("rank")]
		public int Rank { get; set; }

		[JsonProperty("legend_rank")]
		public int LegendRank { get; set; }

		[JsonProperty("game_type")]
		public BnetGameType GameType { get; set; }
	}

	public class BoardState
	{
		[JsonProperty("player")]
		public BoardStatePlayer Player { get; set; }

		[JsonProperty("opponent")]
		public BoardStatePlayer Opponent { get; set; }

		public bool Equals(BoardState boardState)
		{
			if(!Player?.Equals(boardState?.Player) ?? false)
				return false;
			if(!Opponent?.Equals(boardState?.Opponent) ?? false)
				return false;
			return true;
		}
	}

	public class BoardStatePlayer
	{
		[JsonProperty("board", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int[] Board { get; set; }

		[JsonProperty("hand", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public BoardStateHand Hand { get; set; }

		[JsonProperty("deck", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public BoardStateDeck Deck { get; set; }

		[JsonProperty("hero", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Hero { get; set; }

		[JsonProperty("hero_power", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int HeroPower { get; set; }

		[JsonProperty("weapon", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Weapon { get; set; }

		[JsonProperty("fatigue", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Fatigue { get; set; }

		[JsonProperty("secrets", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int[] Secrets { get; set; }

		[JsonProperty("quest", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public BoardStateQuest Quest { get; set; }

		public bool Equals(BoardStatePlayer other)
		{
			if(Hero != other?.Hero)
				return false;
			if(HeroPower != other.HeroPower)
				return false;
			if(Weapon != other.Weapon)
				return false;
			if(!Board?.SequenceEqual(other.Board) ?? false)
				return false;
			if(!Secrets?.SequenceEqual(other.Secrets) ?? false)
				return false;
			if(!Hand?.Equals(other.Hand) ?? false)
				return false;
			if(!Deck?.Equals(other.Deck) ?? false)
				return false;
			if(!Quest?.Equals(other.Quest) ?? false)
				return false;
			return true;
		}
	}

	public class BoardStateQuest
	{
		[JsonProperty("dbfId")]
		public int DbfId { get; set; }

		[JsonProperty("progress")]
		public int Progress { get; set; }

		[JsonProperty("total")]
		public int Total { get; set; }

		public bool Equals(BoardStateQuest other)
		{
			if(DbfId != other?.DbfId)
				return false;
			if(Progress != other.Progress)
				return false;
			if(Total != other.Total)
				return false;
			return true;
		}
	}

	public class BoardStateHand
	{
		[JsonProperty("cards", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int[] Cards { get; set; }

		[JsonProperty("size")]
		public int Size { get; set; }

		public bool Equals(BoardStateHand other)
		{
			if(Size != other?.Size)
				return false;
			if(!Cards?.SequenceEqual(other.Cards) ?? false)
				return false;
			return true;
		}
	}

	public class BoardStateDeck
	{
		[JsonProperty("cards", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<int[]> Cards { get; set; }

		[JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Name { get; set; }

		[JsonProperty("hero", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Hero { get; set; }

		[JsonProperty("format", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public FormatType Format { get; set; }

		[JsonProperty("wins", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Wins { get; set; }

		[JsonProperty("losses", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Losses { get; set; }

		[JsonProperty("size")]
		public int Size { get; set; }

		public bool Equals(BoardStateDeck other)
		{
			if(Size != other?.Size)
				return false;
			if(Name != other.Name)
				return false;
			if(Hero != other.Hero)
				return false;
			if(Format != other.Format)
				return false;
			if(Wins != other.Wins)
				return false;
			if(Losses != other.Losses)
				return false;
			if((Cards == null) != (other.Cards == null))
				return false;
			if(Cards != null)
			{
				if(Cards.Count != other.Cards.Count)
					return false;
				if(Cards.Any(card => !other.Cards.Any(card.SequenceEqual)))
					return false;
			}
			return true;
		}
	}
}
