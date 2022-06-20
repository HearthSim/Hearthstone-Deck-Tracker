using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Battlegrounds
{
	[XmlRoot("BgsLastGames")]
	public sealed class BattlegroundsLastGames
	{
		private static readonly Lazy<BattlegroundsLastGames> LazyInstance = new Lazy<BattlegroundsLastGames>(Load);

		public static BattlegroundsLastGames Instance = LazyInstance.Value;

		[XmlElement("Game")]
		public List<GameItem> Games { get; set; } = new List<GameItem>();

		private static string ConfigPath => Path.Combine(Config.Instance.ConfigDir, "BgsLastGames.xml");

		private static BattlegroundsLastGames Load()
		{
			if(!File.Exists(ConfigPath))
				return new BattlegroundsLastGames();
			try
			{
				return XmlManager<BattlegroundsLastGames>.Load(ConfigPath);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			return new BattlegroundsLastGames();
		}

		private async Task<string?> GetPlayerId()
		{
			var accountId = await Helper.RetryWhileNull(Reflection.GetAccountId, 2, 3000);
			return accountId != null ? $"{accountId.Hi}_{accountId.Lo}" : null;
		}

		public async Task<List<GameItem>> PlayerGames()
		{
			var playerId = await GetPlayerId();
			if (playerId == null)
				return new List<GameItem>();
			
			return Games.Where(g => g.Player == null || g.Player == playerId).ToList();
		}

		public async void AddGame(
			string startTime, string endTime, string hero, int rating, int ratingAfter, int placemenent, Entity[] finalBoard, bool friendlyGame, bool save = true
		)
		{
			var playerId = await GetPlayerId();
			if(playerId == null)
			{
				Log.Info("Unable to save the game. User account can not found...");
				return;
			}
			
			RemoveGame(startTime, false);
			Games.Add(new GameItem(startTime, endTime, hero, rating, ratingAfter, placemenent, finalBoard, friendlyGame, playerId));
			if(save)
				Save();
		}

		public void RemoveGame(string startTime, bool save = true)
		{
			var existing = Games.FirstOrDefault(x => x.StartTime != null && x.StartTime.Equals(startTime));
			if(existing != null)
				Games.Remove(existing);
			if(save)
				Save();
		}

		public static void Save()
		{
			try
			{
				XmlManager<BattlegroundsLastGames>.Save(ConfigPath, Instance);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public void Reset()
		{
			Games.Clear();
			Save();
		}

		public class GameItem
		{
			public GameItem(string startTime, string endTime, string hero, int rating, int ratingAfter, int placemenent, Entity[] finalBoard, bool friendlyGame, string player)
			{
				StartTime = startTime;
				EndTime = endTime;
				Hero = hero;
				Rating = rating;
				RatingAfter = ratingAfter;
				Placement = placemenent;
				FinalBoard = new FinalBoardItem(finalBoard);
				FriendlyGame = friendlyGame;
				Player = player;
			}

			public GameItem()
			{
			}

			[XmlAttribute("Player")]
			public string? Player { get; set; }

			[XmlAttribute("StartTime")]
			public string? StartTime { get; set; }

			[XmlAttribute("EndTime")]
			public string? EndTime { get; set; }

			[XmlAttribute("Hero")]
			public string? Hero { get; set; }

			[XmlAttribute("Rating")]
			public int Rating { get; set; }

			[XmlAttribute("RatingAfter")]
			public int RatingAfter { get; set; }

			[XmlAttribute("Placemenent")]
			public int Placement { get; set; }

			[XmlElement("FinalBoard")]
			public FinalBoardItem? FinalBoard { get; set; }

			[XmlAttribute("FriendlyGame")]
			public bool FriendlyGame { get; set; }

		}

		public class FinalBoardItem
		{
			public FinalBoardItem(Entity[] finalBoard)
			{
				FinalBoard = finalBoard.Select(e => new MinionItem(e)).ToList();
			}

			public FinalBoardItem()
			{
			}

			[XmlElement("Minion")]
			public List<MinionItem>? FinalBoard { get; set; }

		}

		public class MinionItem
		{
			public MinionItem(Entity entity)
			{
				CardId = entity.CardId;
				Tags = entity.Tags.Select(t => new TagItem(t)).ToList();
			}

			public MinionItem()
			{
			}

			public string? CardId { get; set; }

			[XmlElement("Tags")]
			public List<TagItem>? Tags { get; set; }

		}

		public class TagItem
		{
			public TagItem(KeyValuePair<GameTag, int> pair)
			{
				Tag = (int)pair.Key;
				Value = pair.Value;
			}

			public TagItem()
			{
			}

			[XmlAttribute("Tag")]
			public int Tag { get; set; }


			[XmlAttribute("Value")]
			public int Value { get; set; }

		}
	}
}
