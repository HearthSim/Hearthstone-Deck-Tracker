using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.Utility;
using Hearthstone_Deck_Tracker.Stats;
using HSReplay;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public static class UploadMetaDataGenerator
	{
		public static UploadMetaData Generate(GameMetaData? gameMetaData, GameStats? game)
		{
			var metaData = new UploadMetaData();
			var players = GetPlayerInfo(game);
			if (players != null)
			{
				if (game?.GameMode == GameMode.Battlegrounds || game?.GameMode == GameMode.Mercenaries)
					metaData.Players = players;
				else
				{
					metaData.Player1 = players.FirstOrDefault(x => x.Id == 1);
					metaData.Player2 = players.FirstOrDefault(x => x.Id == 2);
				}
			}
			if(gameMetaData?.ServerInfo?.GameHandle > 0)
				metaData.GameHandle = gameMetaData.ServerInfo.GameHandle.ToString();
			if(gameMetaData?.ServerInfo?.ClientHandle > 0)
				metaData.ClientHandle = gameMetaData.ServerInfo.ClientHandle.ToString();
			if(!string.IsNullOrEmpty(gameMetaData?.ServerInfo?.Version))
				metaData.ServerVersion = gameMetaData!.ServerInfo!.Version;
			if(game?.StartTime > DateTime.MinValue)
				metaData.MatchStart = game.StartTime.ToString("o");
			if(game != null)
				metaData.GameType = (int)HearthDbConverter.GetBnetGameType(game.GameType, game.Format);
			if(game?.Format != null)
				metaData.Format = (int)HearthDbConverter.GetFormatType(game.Format);
			metaData.SpectatorMode = game?.GameMode == GameMode.Spectator;
			metaData.Reconnected = gameMetaData?.Reconnected ?? false;
			metaData.Resumable = gameMetaData?.ServerInfo?.Resumable ?? false;
			metaData.FriendlyPlayerId = game?.FriendlyPlayerId > 0 ? game.FriendlyPlayerId : (int?)null;
			var scenarioId = game?.ScenarioId ?? gameMetaData?.ServerInfo?.Mission;
			if(scenarioId > 0)
				metaData.ScenarioId = scenarioId;
			var build = gameMetaData?.HearthstoneBuild;
			if(build == null || build == 0)
				build = game?.HearthstoneBuild;
			if((build == null || build == 0) && game != null)
				build = BuildDates.GetByDate(game.StartTime);
			if(build > 0)
				metaData.HearthstoneBuild = build;
			if(game?.BrawlSeasonId > 0)
				metaData.BrawlSeason = game.BrawlSeasonId;
			if(game?.RankedSeasonId > 0)
				metaData.LadderSeason = game.RankedSeasonId;
			if(gameMetaData?.TwitchVodData != null)
				metaData.TwitchVod = gameMetaData.TwitchVodData;
			if(game?.LeagueId > 0)
				metaData.LeagueId = game.LeagueId;
			if(game?.GameMode == GameMode.Battlegrounds)
				metaData.BattlegroundsRaces = game.BattlegroundsRaces?.Cast<int>().OrderBy(x => x).ToArray();
			if(game?.GameMode == GameMode.Mercenaries)
			{
				if(game?.MercenariesBountyRunRewards?.Count > 0)
				{
					metaData.MercenariesRewards = game.MercenariesBountyRunRewards
						.Select(x => new UploadMetaData.MercenaryReward() { Id = x.Id, Coins = x.Coins})
						.ToList();
				}
				if(!string.IsNullOrEmpty(game?.MercenariesBountyRunId))
				{
					metaData.MercenariesBountyRunId = game!.MercenariesBountyRunId;
					metaData.MercenariesBountyRunTurnsTaken = game!.MercenariesBountyRunTurnsTaken;
					metaData.MercenariesBountyRunCompletedNodes = game!.MercenariesBountyRunCompletedNodes;
				}
			}
			return metaData;
		}

		private static List<UploadMetaData.Player>? GetPlayerInfo(GameStats? game)
		{
			if(game == null || game.FriendlyPlayerId == 0)
				return null;

			var friendly = new UploadMetaData.Player();
			var opposing = new UploadMetaData.Player();

			friendly.Id = game.FriendlyPlayerId;
			opposing.Id = game.OpponentPlayerId;

			if(game.PlayerCardbackId > 0)
				friendly.Cardback = game.PlayerCardbackId;

			if(game.GameMode == GameMode.Ranked)
			{
				if(game.Rank > 0)
					friendly.Rank = game.Rank;
				if(game.LegendRank > 0)
					friendly.LegendRank = game.LegendRank;
				if(game.Stars > 0)
					friendly.Stars = game.Stars;
				if(game.StarLevel > 0)
					friendly.StarLevel = game.StarLevel;
				if(game.StarMultiplier > 0)
					friendly.StarMultiplier = game.StarMultiplier;

				if(game.StarsAfter > 0)
					friendly.StarsAfter = game.StarsAfter;
				if(game.StarLevelAfter > 0)
					friendly.StarLevelAfter = game.StarLevelAfter;
				if(game.LegendRankAfter > 0)
					friendly.LegendRankAfter = game.LegendRankAfter;

				if(game.OpponentRank > 0)
					opposing.Rank = game.OpponentRank;
				if(game.OpponentLegendRank > 0)
					opposing.LegendRank = game.OpponentLegendRank;
				if(game.OpponentStarLevel > 0)
					opposing.StarLevel = game.OpponentStarLevel;
			}

			var playerDeckSize = game.PlayerCards.Sum(x => x.Count);
			if(game.GameMode == GameMode.Battlegrounds)
			{
				if(game.BattlegroundsRating > 0)
					friendly.BattlegroundsRating = game.BattlegroundsRating;
				if(game.BattlegroundsRatingAfter > 0)
					friendly.BattlegroundsRatingAfter = game.BattlegroundsRatingAfter;
			}
			else if(game.GameMode == GameMode.Mercenaries)
			{
				if(game.MercenariesRating > 0)
					friendly.MercenariesRating = game.MercenariesRating;
				if(game.MercenariesRatingAfter > 0)
					friendly.MercenariesRatingAfter = game.MercenariesRatingAfter;
			}
			else if(playerDeckSize == 30 || playerDeckSize == 40 || game.IsPVPDungeonMatch || game.IsDungeonMatch == true && game.DeckId != Guid.Empty)
			{
				friendly.DeckList = game.PlayerCards.Where(x => x.Id != Database.UnknownCardId).SelectMany(x => Enumerable.Repeat(x.Id, x.Count)).ToArray();
				if(game.HsDeckId > 0)
					friendly.DeckId = game.HsDeckId;
			}
			if(game.GameMode == GameMode.Arena)
			{
				if(game.ArenaWins > 0)
					friendly.Wins = game.ArenaWins;
				if(game.ArenaLosses > 0)
					friendly.Losses = game.ArenaLosses;
			}
			else if(game.GameMode == GameMode.Brawl)
			{
				if(game.BrawlWins > 0)
					friendly.Wins = game.BrawlWins;
				if(game.BrawlLosses > 0)
					friendly.Losses = game.BrawlLosses;
			}
			if(game.OpponentCardbackId > 0)
				opposing.Cardback = game.OpponentCardbackId;

			return new List<UploadMetaData.Player>() {
				friendly,
				opposing
			};
		}
	}

	public class PlayerInfo
	{
		public UploadMetaData.Player Player1 { get; }
		public UploadMetaData.Player Player2 { get; }
		public int FriendlyPlayerId { get; }
		public PlayerInfo(UploadMetaData.Player player1, UploadMetaData.Player player2, int friendlyPlayerId = -1)
		{
			Player1 = player1;
			Player2 = player2;
			FriendlyPlayerId = friendlyPlayerId;
		}
	}
}
