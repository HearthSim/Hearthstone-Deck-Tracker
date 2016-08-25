using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.Utility;
using Hearthstone_Deck_Tracker.Stats;
using HSReplay;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public static class UploadMetaDataGenerator
	{
		public static UploadMetaData Generate(string[] log, GameMetaData gameMetaData, GameStats game)
		{
			var metaData = new UploadMetaData();
			var playerInfo = GetPlayerInfo(log, game);
			if(playerInfo != null)
			{
				metaData.Player1 = playerInfo.Player1;
				metaData.Player2 = playerInfo.Player2;
			}
			if(!string.IsNullOrEmpty(gameMetaData?.ServerInfo?.Address))
				metaData.ServerIp = gameMetaData.ServerInfo.Address;
			if(gameMetaData?.ServerInfo?.Port > 0)
				metaData.ServerPort = gameMetaData.ServerInfo.Port.ToString();
			if(gameMetaData?.ServerInfo?.GameHandle > 0)
				metaData.GameHandle = gameMetaData.ServerInfo.GameHandle.ToString();
			if(gameMetaData?.ServerInfo?.ClientHandle > 0)
				metaData.ClientHandle = gameMetaData.ServerInfo.ClientHandle.ToString();
			if(!string.IsNullOrEmpty(gameMetaData?.ServerInfo?.SpectatorPassword))
				metaData.SpectatePassword = gameMetaData.ServerInfo.SpectatorPassword;
			if(!string.IsNullOrEmpty(gameMetaData?.ServerInfo?.AuroraPassword))
				metaData.AuroraPassword = gameMetaData.ServerInfo.AuroraPassword;
			if(!string.IsNullOrEmpty(gameMetaData?.ServerInfo?.Version))
				metaData.ServerVersion = gameMetaData.ServerInfo.Version;
			if(game?.StartTime > DateTime.MinValue)
				metaData.MatchStart = game.StartTime.ToString("o");
			if(game != null)
				metaData.GameType = game.GameType != GameType.GT_UNKNOWN ? (int)HearthDbConverter.GetBnetGameType(game.GameType, game.Format) : (int)HearthDbConverter.GetGameType(game.GameMode, game.Format);
			if(game?.Format != null)
				metaData.Format = (int)HearthDbConverter.GetFormatType(game.Format);
			metaData.SpectatorMode = game?.GameMode == GameMode.Spectator;
			metaData.Reconnected = gameMetaData?.Reconnected ?? false;
			metaData.Resumable = gameMetaData?.ServerInfo?.Resumable ?? false;
			metaData.FriendlyPlayerId = game?.FriendlyPlayerId > 0 ? game.FriendlyPlayerId : (playerInfo?.FriendlyPlayerId > 0 ? playerInfo?.FriendlyPlayerId : null);
			var scenarioId = game?.ScenarioId ?? gameMetaData?.ServerInfo?.Mission;
			if(scenarioId > 0)
				metaData.ScenarioId = scenarioId;
			var build = gameMetaData?.HearthstoneBuild ?? game?.HearthstoneBuild ?? (game != null ? BuildDates.GetByDate(game.StartTime) : null);
			if(build > 0)
				metaData.HearthstoneBuild = build;
			if(game?.BrawlSeasonId > 0)
				metaData.TavernBrawlSeason = game.BrawlSeasonId;
			return metaData;
		}

		private static PlayerInfo GetPlayerInfo(string[] log, GameStats game)
		{
			var friendly = new UploadMetaData.Player();
			var opposing = new UploadMetaData.Player();

			if(game?.Rank > 0)
				friendly.Rank = game.Rank;
			if(game?.LegendRank > 0)
				friendly.LegendRank = game.LegendRank;
			if(game?.PlayerCardbackId > 0)
				friendly.Cardback = game.PlayerCardbackId;
			if(game?.Stars > 0)
				friendly.Stars = game.Stars;
			if(game?.PlayerCards.Sum(x => x.Count) == 30 && game?.PlayerCards.Sum(x => x.Unconfirmed) <= 24)
			{
				friendly.DeckList = game.PlayerCards.SelectMany(x => Enumerable.Repeat(x.Id, x.Count)).ToArray();
				if(game.HsDeckId > 0)
					friendly.DeckId = game.HsDeckId;
			}

			if(game?.OpponentRank > 0)
				opposing.Rank = game.OpponentRank;
			if(game?.OpponentLegendRank > 0)
				opposing.LegendRank = game.OpponentLegendRank;
			if(game?.OpponentCardbackId > 0)
				opposing.Cardback = game.OpponentCardbackId;

			if(game?.FriendlyPlayerId > 0)
			{
				return new PlayerInfo(game.FriendlyPlayerId == 1 ? friendly : opposing,
					game.FriendlyPlayerId == 2 ? friendly : opposing);
			}
			var player1Name = GetPlayer1Name(log);
			if(player1Name == game?.PlayerName)
				return new PlayerInfo(friendly, opposing, 1);
			if(player1Name == game?.OpponentName)
				return new PlayerInfo(opposing, friendly, 2);
			return null;
		}

		private static string GetPlayer1Name(IEnumerable<string> log)
		{
			foreach(var line in log)
			{
				var match = Regex.Match(line, @"TAG_CHANGE Entity=(?<name>(.+)) tag=CONTROLLER value=1");
				if(!match.Success)
					continue;
				return match.Groups["name"].Value;
			}
			return null;
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
