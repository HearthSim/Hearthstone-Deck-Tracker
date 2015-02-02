#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	public class HearthStatsManager
	{
		public const int RetryDelay = 5000;
		public const int VersionDelay = 1000;

		public static async Task<PostResult> UploadMatchAsync(GameStats game, Deck deck, bool saveFilesAfter = true)
		{
			//await Task.Delay(1000);
			//return true;
			Logger.WriteLine("trying to upload match: " + game, "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return PostResult.Failed;
			}
			if(!HearthStatsAPI.IsValidGame(game))
				return PostResult.Failed;
			if(!deck.HasHearthStatsId)
			{
				Logger.WriteLine("...deck has no HearthStats id, uploading deck", "HearthStatsSync");
				var success = await UploadDeckAsync(deck);
				if(!success.Success)
				{
					Logger.WriteLine("error: deck could not be uploaded or did not return an id. Can not upload match.", "HearthStatsSync");
					return PostResult.Failed;
				}
			}
			var result = await HearthStatsAPI.PostGameResultAsync(game, deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to upload match: " + game, "HearthStatsSync");
				result = await HearthStatsAPI.PostGameResultAsync(game, deck);
			}
			if(result.Success && saveFilesAfter)
				DeckStatsList.Save();
			return result;
		}

		public static PostResult UploadMatch(GameStats game, Deck deck, bool saveFilesAfter = true)
		{
			return UploadMatchAsync(game, deck, saveFilesAfter).Result;
		}

		public static async Task<List<Deck>> DownloadDecksAsync(bool forceAll = false)
		{
			Logger.WriteLine("trying do download decks", "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return null;
			}
			var decks = await HearthStatsAPI.GetDecksAsync(forceAll ? 0 : Config.Instance.LastHearthStatsDecksSync);
			//Config.Instance.LastHearthStatsDecksSync = DateTime.Now.ToUnixTime();
			//Config.Save();

			return decks ?? new List<Deck>();
		}

		public static async Task<List<GameStats>> DownloadGamesAsync(bool forceAll = false)
		{
			//await Task.Delay(1000);
			//return new List<GameStats>();
			Logger.WriteLine("trying do download decks", "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return null;
			}
			var games = await HearthStatsAPI.GetGamesAsync(forceAll ? 0 : Config.Instance.LastHearthStatsGamesSync);
			//Config.Instance.LastHearthStatsGamesSync = DateTime.Now.ToUnixTime();
			//Config.Save();

			//save games to decks

			return games;
		}

		public static async Task<bool> DeleteDeckAsync(Deck deck)
		{
			Logger.WriteLine("trying do delete deck " + deck, "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return false;
			}
			var result = await HearthStatsAPI.DeleteDeckAsync(deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 do delete deck " + deck, "HearthStatsSync");
				result = await HearthStatsAPI.DeleteDeckAsync(deck);
			}
			return result.Success;
		}

		public static async Task<PostResult> UploadDeckAsync(Deck deck, bool saveFilesAfter = true)
		{
			//await Task.Delay(1000);
			//return true;
			Logger.WriteLine("trying to upload deck " + deck, "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return PostResult.Failed;
			}

			var first = deck.GetVersion(1, 0);
			var result = await HearthStatsAPI.PostDeckAsync(first);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to upload deck " + deck, "HearthStatsSync");
				result = await HearthStatsAPI.PostDeckAsync(first);
			}
			if(result.Success)
			{
				var versions =
					deck.VersionsIncludingSelf.Where(v => v != new SerializableVersion(1, 0))
					    .Select(v => deck.GetVersion(v.Major, v.Minor))
					    .Where(d => d != null && !d.VersionOnHearthStats)
					    .ToList();
				if(versions.Any())
				{
					foreach(var v in versions)
					{
						//await Task.Delay(VersionDelay);
						await UploadVersionAsync(v, first.HearthStatsId, false);
					}
				}
				if(saveFilesAfter)
					Helper.MainWindow.WriteDecks();
				return PostResult.WasSuccess;
			}
			return PostResult.Failed;
		}

		public static PostResult UploadDeck(Deck deck, bool saveFilesAfter = true)
		{
			return UploadDeckAsync(deck, saveFilesAfter).Result;
		}

		public static async void Sync(bool forceFullSync = false)
		{
			forceFullSync = true;
			var controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Checking HearthStats for new decks...");
			var decks = await DownloadDecksAsync(forceFullSync);
			var localDecks = Helper.MainWindow.DeckList.DecksList;
			var newDecks = decks.Where(deck => localDecks.All(localDeck => localDeck.HearthStatsId != deck.HearthStatsId)).ToList();
			if(newDecks.Any())
			{
				await controller.CloseAsync();
				Helper.MainWindow.FlyoutHearthStatsDownload.IsOpen = true;
				newDecks = await Helper.MainWindow.HearthStatsDownloadDecksControl.LoadDecks(newDecks);
				foreach(var deck in newDecks)
				{
					Helper.MainWindow.DeckList.DecksList.Add(deck);
					Helper.MainWindow.DeckPickerList.AddDeck(deck);
				}
				Helper.MainWindow.WriteDecks();
				Helper.MainWindow.DeckPickerList.UpdateList();
			}

			if(!controller.IsOpen)
				controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Checking HearthStats for new versions...");
			else
				controller.SetMessage("Checking HearthStats for new versions...");
			localDecks = Helper.MainWindow.DeckList.DecksList;
			var versions = await DownloadVersionsAsync(forceFullSync);
			var newVersions =
				versions.Where(v => localDecks.Any(d => d.HasHearthStatsId && d.HearthStatsId == v.HearthStatsId && !d.HasVersion(v.Version)))
				        .OrderBy(v => v.Version)
				        .ToList();
			if(newVersions.Any())
			{
				foreach(var version in newVersions)
				{
					var deck = localDecks.FirstOrDefault(d => d.HasHearthStatsId && d.HearthStatsId == version.HearthStatsId);
					if(deck != null)
					{
						Helper.MainWindow.DeckList.DecksList.Remove(deck);
						deck.Versions.Clear();
						version.Versions.Add(deck);
						Helper.MainWindow.DeckList.DecksList.Add(version);
					}
				}
				Helper.MainWindow.WriteDecks();
			}


			if(!controller.IsOpen)
				controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Checking HearthStats for new matches...");
			else
				controller.SetMessage("Checking HearthStats for new matches...");
			var newGames = await DownloadGamesAsync(forceFullSync);
			if(newGames.Any())
			{
				foreach(var game in newGames)
				{
					var deck =
						Helper.MainWindow.DeckList.DecksList.FirstOrDefault(d => d.HasHearthStatsId && d.HearthStatsId == game.HearthStatsDeckId);
					if(deck != null && deck.DeckStats.Games.All(g => g.HearthStatsId != game.HearthStatsId))
						deck.DeckStats.AddGameResult(game);
				}
				DeckStatsList.Save();
			}


			controller.SetMessage("Checking for new local decks...");
			newDecks = localDecks.Where(deck => !deck.HasHearthStatsId && !deck.SyncWithHearthStats.HasValue).ToList();
			if(newDecks.Any())
			{
				await controller.CloseAsync();
				Helper.MainWindow.FlyoutHearthStatsUpload.IsOpen = true;
				newDecks = await Helper.MainWindow.HearthStatsUploadDecksControl.LoadDecks(newDecks);
				controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Uploading new decks...");
				await Task.Run(() => { Parallel.ForEach(newDecks, deck => UploadDeck(deck, false)); });
				Helper.MainWindow.WriteDecks(); //save new ids
			}


			controller.SetMessage("Checking for new local versions...");
			var localNewVersions =
				localDecks.Where(x => x.HasHearthStatsId)
				          .SelectMany(
				                      x =>
				                      x.Versions.Where(v => !v.VersionOnHearthStats)
				                       .Select(v => new {version = v, hearthStatsId = x.HearthStatsId}))
				          .ToList();
			if(localNewVersions.Any())
			{
				controller.SetMessage("Uploading new versions...");
				//this can't happen in parallel (?)
				foreach(var v in localNewVersions)
				{
					var result = await UploadVersionAsync(v.version, v.hearthStatsId, false);
					if(!result.Success && result.Retry)
					{
						await Task.Delay(RetryDelay);
						await UploadVersionAsync(v.version, v.hearthStatsId, false);
					}
				}
				Helper.MainWindow.WriteDecks();
			}

			controller.SetMessage("Checking for new local matches...");

			var newMatches =
				Helper.MainWindow.DeckList.DecksList.Where(d => d.SyncWithHearthStats.HasValue && d.SyncWithHearthStats.Value)
				      .SelectMany(d => d.DeckStats.Games.Where(g => !g.HasHearthStatsId).Select(g => new {game = g, deck = d}))
				      .ToList();
			if(newGames.Any())
			{
				controller.SetMessage("Uploading new matches...");
				await Task.Run(() => { Parallel.ForEach(newMatches, match => UploadMatch(match.game, match.deck, false)); });
				DeckStatsList.Save();
			}
			Config.Instance.LastHearthStatsDecksSync = DateTime.Now.ToUnixTime();
			Config.Instance.LastHearthStatsGamesSync = DateTime.Now.ToUnixTime();
			await controller.CloseAsync();
		}

		public static async Task<List<Deck>> DownloadVersionsAsync(bool forceAll = false)
		{
			await Task.Delay(100);
			return new List<Deck>();
		}

		public static async Task<PostResult> UploadVersionAsync(Deck deck, string hearthStatsId, bool saveFilesAfter = true)
		{
			Logger.WriteLine("trying to upload version " + deck.Version + " of " + deck, "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return PostResult.Failed;
			}
			var result = await HearthStatsAPI.PostVersionAsync(deck, hearthStatsId);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to upload version " + deck.Version + " of " + deck, "HearthStatsSync");
				result = await HearthStatsAPI.PostVersionAsync(deck, hearthStatsId);
			}
			if(result.Success && saveFilesAfter)
				Helper.MainWindow.WriteDecks();
			return result;
		}

		public static PostResult UploadVersion(Deck deck, string hearthStatsId, bool saveFilesAfter = true)
		{
			return UploadVersionAsync(deck, hearthStatsId, saveFilesAfter).Result;
		}

		public static async Task<PostResult> DeleteMatchAsync(GameStats game)
		{
			Logger.WriteLine("trying do delete game " + game, "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return PostResult.Failed;
			}
			var result = await HearthStatsAPI.DeleteMatchAsync(game);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 do delete game " + game, "HearthStatsSync");
				result = await HearthStatsAPI.DeleteMatchAsync(game);
			}
			return result;
		}
	}
}