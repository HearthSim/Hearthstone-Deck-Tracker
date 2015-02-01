#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	internal class HearthStatsSync
	{
		public static async Task<bool> UploadMatchAsync(GameStats game, Deck deck, bool saveFilesAfter = true)
		{
			Logger.WriteLine("trying to upload match: " + game, "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return false;
			}
			if(!HearthStatsAPI.IsValidGame(game))
				return false;
			if(!deck.HasHearthStatsId)
			{
				Logger.WriteLine("...deck has no HearthStats id, uploading deck", "HearthStatsSync");
				var success = await HearthStatsAPI.PostDeckAsync(deck);
				if(!success)
				{
					Logger.WriteLine("error: deck could not be uploaded or did not return an id. Can not upload match.", "HearthStatsSync");
					return false;
				}
			}
			await HearthStatsAPI.PostGameResultAsync(game, deck);
			Logger.WriteLine("upload complete", "HearthStatsSync");
			if(saveFilesAfter)
			{
				DeckStatsList.Save();
				Helper.MainWindow.WriteDecks();
			}
			return true;
		}

		public static async Task<List<Deck>> DownloadDecksAsync(bool forceAll = false)
		{
			Logger.WriteLine("trying do download decks", "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return null;
			}
			var json = await HearthStatsAPI.GetDecksAsync(forceAll ? 0 : Config.Instance.LastHearthStatsDecksSync);
			Config.Instance.LastHearthStatsDecksSync = DateTime.Now.ToUnixTime();
			Config.Save();

			//convert json to deck list

			return new List<Deck>();
		}

		public static async Task<List<GameStats>> DownloadGamesAsync(bool forceAll = false)
		{
			Logger.WriteLine("trying do download decks", "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return null;
			}
			var json = await HearthStatsAPI.GetGamesAsync(forceAll ? 0 : Config.Instance.LastHearthStatsGamesSync);
			Config.Instance.LastHearthStatsGamesSync = DateTime.Now.ToUnixTime();
			Config.Save();

			var gameObjs = (dynamic[])JsonConvert.DeserializeObject(json);
			var games =
				gameObjs.Select(
				                x =>
				                new GameStats
				                {
					                Result = x.result,
					                GameMode = x.mode,
					                PlayerHero = x.@class,
					                OpponentHero = x.oppclass,
					                OpponentName = x.oppname,
					                Turns = x.numturns,
					                Coin = x.coin,
					                HearthStatsId = x.id,
					                HearthStatsDeckId = x.deck_id,
					                Note = x.notes,
					                Rank = x.ranklvl,
					                StartTime = Helper.FromUnixTime(long.Parse(x.startTime.ToString())),
					                EndTime = Helper.FromUnixTime(long.Parse(x.startTime.ToString()) + int.Parse(x.duration.ToString()))
				                });

			return games.ToList();
		}

		public static async Task<bool> DeleteDeckAsync(Deck deck)
		{
			Logger.WriteLine("trying do delete deck " + deck, "HearthStatsSync");
			return await HearthStatsAPI.DeleteDeckAsync(deck);
		}

		///public static async Task<bool> UploadDeckAsync(Deck deck)
		//{

		//}
	}
}