using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
			var playerInfo = GetPlayerInfo(log, game);
			return new UploadMetaData
			{
				ServerIp = gameMetaData?.ServerInfo?.Address,
				ServerPort = gameMetaData?.ServerInfo?.Port.ToString(),
				GameHandle = gameMetaData?.ServerInfo?.GameHandle.ToString(),
				ClientHandle = gameMetaData?.ServerInfo?.ClientHandle.ToString(),
				Reconnected = gameMetaData?.Reconnected ?? false,
				Resumable = gameMetaData?.ServerInfo?.Resumable ?? false,
				SpectatePassword = gameMetaData?.ServerInfo?.SpectatorPassword,
				AuroraPassword = gameMetaData?.ServerInfo?.AuroraPassword,
				ServerVersion = gameMetaData?.ServerInfo?.Version,
				MatchStart = game?.StartTime != DateTime.MinValue ? game?.StartTime.ToString("o") : null,
				HearthstoneBuild = gameMetaData?.HearthstoneBuild ?? game?.HearthstoneBuild ?? (game != null ? BuildDates.GetByDate(game.StartTime) : null),
				GameType = game != null ? (int)HearthDbConverter.GetGameType(game.GameMode, game.Format) : (int?)null,
				SpectatorMode = game?.GameMode == GameMode.Spectator,
				FriendlyPlayerId = game?.FriendlyPlayerId > 0 ? game.FriendlyPlayerId : (playerInfo?.FriendlyPlayerId > 0 ? playerInfo?.FriendlyPlayerId : null),
				ScenarioId = game?.ScenarioId ?? gameMetaData?.ServerInfo?.Mission,
				Format = game?.Format != null ? (int)HearthDbConverter.GetFormatType(game.Format) : (int?)null,
				Player1 = playerInfo?.Player1,
				Player2 = playerInfo?.Player2
			};
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
