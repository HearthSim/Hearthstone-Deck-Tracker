#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	internal class HearthStatsManager
	{
		public static async Task<bool> UploadMatchAsync(GameStats game, Deck deck, bool saveFilesAfter = true)
		{
			await Task.Delay(1000);
			return true;
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

		public static bool UploadMatch(GameStats game, Deck deck, bool saveFilesAfter = true)
		{
			return UploadMatchAsync(game, deck, saveFilesAfter).Result;
		}

		public static async Task<List<Deck>> DownloadDecksAsync(bool forceAll = false)
		{
			await Task.Delay(1000);
			return new List<Deck>();
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
			await Task.Delay(1000);
			return new List<GameStats>();
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

		public static async Task<bool> UploadDeckAsync(Deck deck)
		{
			await Task.Delay(1000);
			return true;
			Logger.WriteLine("trying to upload deck " + deck, "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return false;
			}
			return await HearthStatsAPI.PostDeckAsync(deck);
		}

		public static bool UploadDeck(Deck deck)
		{
			return UploadDeckAsync(deck).Result;
		}

		///public static async Task<bool> UploadDeckAsync(Deck deck)
		//{

		//}
		public static async void Sync()
		{
			var controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Checking HearthStats for new decks...");
			var decks = await DownloadDecksAsync();
			var localDecks = Helper.MainWindow.DeckList.DecksList;
			var newDecks = decks.Where(deck => localDecks.All(localDeck => localDeck.HearthStatsId != deck.HearthStatsId)).ToList();
			if(newDecks.Any())
			{
				await controller.CloseAsync();
				Helper.MainWindow.FlyoutHearthStatsDownload.IsOpen = true;
				newDecks = await Helper.MainWindow.HearthStatsDownloadDecksControl.LoadDecks(newDecks);
				foreach(var deck in newDecks)
					Helper.MainWindow.DeckList.DecksList.Add(deck);
				Helper.MainWindow.WriteDecks();
			}
			if(!controller.IsOpen)
				controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Checking HearthStats for new matches...");
			else
				controller.SetMessage("Checking HearthStats for new matches...");

			var newGames = await DownloadGamesAsync();
			foreach(var game in newGames)
			{
				var deck = Helper.MainWindow.DeckList.DecksList.FirstOrDefault(d => d.HasHearthStatsId && d.HearthStatsId == game.HearthStatsDeckId);
				if(deck != null)
					deck.DeckStats.AddGameResult(game);
			}
			controller.SetMessage("Checking for new local decks...");
			newDecks = localDecks.Where(deck => !deck.HasHearthStatsId && !deck.SyncWithHearthStats.HasValue).ToList();
			//TODO: versions!
			if(newDecks.Any())
			{
				await controller.CloseAsync();
				Helper.MainWindow.FlyoutHearthStatsUpload.IsOpen = true;
				newDecks = await Helper.MainWindow.HearthStatsUploadDecksControl.LoadDecks(newDecks);
				controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Uploading new decks...");
				Parallel.ForEach(newDecks, deck => UploadDeck(deck));
			}
			controller.SetMessage("Checking for new local matches...");

			var newMatches =
				Helper.MainWindow.DeckList.DecksList.Where(d => d.SyncWithHearthStats.HasValue && d.SyncWithHearthStats.Value)
				      .SelectMany(d => d.DeckStats.Games.Where(g => !g.HasHearthStatsId).Select(g => new { game = g, deck = d}))
				      .ToList();
			if(newGames.Any())
			{
				controller.SetMessage("Uploading new matches...");
				Parallel.ForEach(newMatches, match => UploadMatch(match.game, match.deck, false));
			}

			await controller.CloseAsync();
		}
	}
}