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
		private static bool _syncInProgress;

		public static async Task<PostResult> UploadMatchAsync(GameStats game, Deck deck, bool saveFilesAfter = true, bool background = false)
		{
			//await Task.Delay(1000);
			//return true;
			Logger.WriteLine("trying to upload match: " + game, "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return PostResult.Failed;
			}
			if(!HearthStatsAPI.IsValidGame(game))
				return PostResult.Failed;
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = true;
			if(!deck.HasHearthStatsId)
			{
				Logger.WriteLine("...deck has no HearthStats id, uploading deck", "HearthStatsManager");
				var success = await UploadDeckAsync(deck);
				if(!success.Success)
				{
					Logger.WriteLine("error: deck could not be uploaded or did not return an id. Can not upload match.", "HearthStatsManager");
					if(background)
						Helper.MainWindow.ProgressRingTitleBar.IsActive = false;
					return PostResult.Failed;
				}
			}
			var result = await HearthStatsAPI.PostGameResultAsync(game, deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to upload match: " + game, "HearthStatsManager");
				result = await HearthStatsAPI.PostGameResultAsync(game, deck);
			}
			if(result.Success && saveFilesAfter)
				DeckStatsList.Save();
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = false;
			if(result.Success)
				Logger.WriteLine("success uploading game " + game, "HearthStatsManager");
			return result;
		}

		public static PostResult UploadMatch(GameStats game, Deck deck, bool saveFilesAfter = true)
		{
			return UploadMatchAsync(game, deck, saveFilesAfter).Result;
		}

		public static async Task<List<Deck>> DownloadDecksAsync(bool forceAll = false)
		{
			Logger.WriteLine("trying to download decks", "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return null;
			}
			var decks = await HearthStatsAPI.GetDecksAsync(forceAll ? 0 : Config.Instance.LastHearthStatsDecksSync);
			//Config.Instance.LastHearthStatsDecksSync = DateTime.Now.ToUnixTime();
			//Config.Save();

			return decks ?? new List<Deck>();
		}

		public static async Task<List<GameStats>> DownloadGamesAsync(bool forceAll = false)
		{
			Logger.WriteLine("trying to download games", "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return null;
			}
			var games = await HearthStatsAPI.GetGamesAsync(forceAll ? 0 : Config.Instance.LastHearthStatsGamesSync);
			return games;
		}

		public static async Task<bool> DeleteDeckAsync(Deck deck)
		{
			Logger.WriteLine("trying to delete deck " + deck, "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return false;
			}
			var result = await HearthStatsAPI.DeleteDeckAsync(deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to delete deck " + deck, "HearthStatsManager");
				result = await HearthStatsAPI.DeleteDeckAsync(deck);
			}
			if(result.Success)
				Logger.WriteLine("success deleting deck " + deck, "HearthStatsManager");
			return result.Success;
		}

		public static async Task<PostResult> UploadDeckAsync(Deck deck, bool saveFilesAfter = true, bool background = false)
		{
			//await Task.Delay(1000);
			//return true;
			Logger.WriteLine("trying to upload deck " + deck, "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return PostResult.Failed;
			}

			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = true;
			var first = deck.GetVersion(1, 0);
			var result = await HearthStatsAPI.PostDeckAsync(first);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to upload deck " + deck, "HearthStatsManager");
				result = await HearthStatsAPI.PostDeckAsync(first);
			}
			if(result.Success)
			{
				var versions =
					deck.VersionsIncludingSelf.Where(v => v != new SerializableVersion(1, 0))
					    .Select(v => deck.GetVersion(v.Major, v.Minor))
					    .Where(d => d != null && !d.HasHearthStatsDeckVersionId)
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
				if(background)
					Helper.MainWindow.ProgressRingTitleBar.IsActive = false;
				return PostResult.WasSuccess;
			}
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = false;
			if(result.Success)
				Logger.WriteLine("success uploading deck " + deck, "HearthStatsManager");
			return PostResult.Failed;
		}

		public static PostResult UploadDeck(Deck deck, bool saveFilesAfter = true)
		{
			return UploadDeckAsync(deck, saveFilesAfter).Result;
		}

		public static async void SyncAsync(bool forceFullSync = false, bool background = false)
		{
			Logger.WriteLine(string.Format("starting sync process: forceFullSync={0}, background={1}", forceFullSync, background),
			                 "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return;
			}
			try
			{
				if(_syncInProgress)
				{
					Logger.WriteLine("error: sync already in progress", "HearthStatsManager");
					return;
				}
				_syncInProgress = true;
				if(background)
					Helper.MainWindow.ProgressRingTitleBar.IsActive = true;

				var controller = background
					                 ? null : await Helper.MainWindow.ShowProgressAsync("Syncing...", "Checking HearthStats for new decks...");
				var decks = await DownloadDecksAsync(forceFullSync);
				var localDecks = Helper.MainWindow.DeckList.DecksList;
				var newDecks = decks.Where(deck => localDecks.All(localDeck => localDeck.HearthStatsId != deck.HearthStatsId)).ToList();
				if(newDecks.Any())
				{
					Helper.MainWindow.FlyoutHearthStatsDownload.IsOpen = true;
					if(!background)
						await controller.CloseAsync();
					newDecks = await Helper.MainWindow.HearthStatsDownloadDecksControl.LoadDecks(newDecks);
					foreach(var deck in newDecks)
					{
						Helper.MainWindow.DeckList.DecksList.Add(deck);
						Helper.MainWindow.DeckPickerList.AddDeck(deck);
					}
					Helper.MainWindow.WriteDecks();
					Helper.MainWindow.DeckPickerList.UpdateList();
					background = false;
				}
				if(!background)
				{
					if(controller == null || !controller.IsOpen)
						controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Checking for new versions...");
					else
						controller.SetMessage("Checking for new versions...");
				}
				var decksWithNewVersions =
					decks.Where(deck => localDecks.All(localDeck => localDeck.HasHearthStatsId && localDeck.Version != deck.Version)).ToList();
				if(decksWithNewVersions.Any()) // TODO: TEST
				{
					foreach(var deck in decksWithNewVersions)
					{
						var currentDeck = localDecks.FirstOrDefault(d => d.HasHearthStatsId && d.HearthStatsId == deck.HearthStatsId);
						if(currentDeck == null)
							continue;
						var originalDeck = (Deck)currentDeck.Clone();
						var versions =
							deck.VersionsIncludingSelf.Where(v => !currentDeck.VersionsIncludingSelf.Contains(v))
							    .OrderBy(v => v)
							    .Select(v => deck.GetVersion(v.Major, v.Minor));
						foreach(var newVersion in versions)
						{
							var clone = (Deck)currentDeck.Clone();
							currentDeck.Version = newVersion.Version;
							currentDeck.SelectedVersion = newVersion.Version;
							currentDeck.HearthStatsDeckVersionId = newVersion.HearthStatsDeckVersionId;
							currentDeck.Versions.Add(clone);
						}
						Helper.MainWindow.DeckPickerList.RemoveDeck(originalDeck);
						Helper.MainWindow.DeckPickerList.AddDeck(currentDeck);
					}
				}
				if(!background)
				{
					if(controller == null || !controller.IsOpen)
						controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Checking for edited decks...");
					else
						controller.SetMessage("Checking for edited decks...");
				}
				foreach(var deck in decks) //TODO: TEST
				{
					var localDeck = localDecks.FirstOrDefault(d => d.HasHearthStatsId && d.HearthStatsId == deck.HearthStatsId);
					if(localDeck != null)
					{
						localDeck.Name = deck.Name;
						localDeck.Tags = deck.Tags;
						localDeck.Note = deck.Note;
						localDeck.Cards.Clear();
						foreach(var card in deck.Cards)
							localDeck.Cards.Add((Card)card.Clone());
					}
					Helper.MainWindow.DeckPickerList.UpdateList();
					Helper.MainWindow.WriteDecks();
				}


				if(!background)
				{
					if(controller == null || !controller.IsOpen)
						controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Checking HearthStats for new matches...");
					else
						controller.SetMessage("Checking HearthStats for new matches...");
				}
				var newGames = await DownloadGamesAsync(forceFullSync);
				if(newGames.Any())
				{
					foreach(var game in newGames)
					{
						var deck =
							Helper.MainWindow.DeckList.DecksList.FirstOrDefault(
							                                                    d =>
							                                                    d.HasHearthStatsId && d.HearthStatsId == game.HearthStatsDeckId
							                                                    && d.HearthStatsDeckVersionId == game.HearthStatsDeckVersionId);
						if(deck != null && deck.DeckStats.Games.All(g => g.HearthStatsId != game.HearthStatsId))
						{
							game.PlayerDeckVersion = deck.Version;
							deck.DeckStats.AddGameResult(game);
						}
					}
					DeckStatsList.Save();
					Helper.MainWindow.DeckPickerList.UpdateList();
				}

				if(!background)
					controller.SetMessage("Checking for new local decks...");
				newDecks = localDecks.Where(deck => !deck.HasHearthStatsId).ToList();
				if(newDecks.Any(d => !d.SyncWithHearthStats.HasValue))
				{
					if(!background)
						await controller.CloseAsync();
					Helper.MainWindow.FlyoutHearthStatsUpload.IsOpen = true;
					newDecks = await Helper.MainWindow.HearthStatsUploadDecksControl.LoadDecks(newDecks);
					controller = await Helper.MainWindow.ShowProgressAsync("Syncing...", "Uploading new decks...");
					await Task.Run(() => { Parallel.ForEach(newDecks, deck => UploadDeck(deck, false)); });
					Helper.MainWindow.WriteDecks(); //save new ids
					background = false;
				}

				if(!background)
					controller.SetMessage("Checking for new local versions...");
				var localNewVersions =
					localDecks.Where(x => x.HasHearthStatsId)
					          .SelectMany(
					                      x =>
					                      x.Versions.Where(v => !v.HasHearthStatsDeckVersionId)
					                       .Select(v => new {version = v, hearthStatsId = x.HearthStatsId}))
					          .ToList();
				if(localNewVersions.Any())
				{
					if(!background)
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
				if(!background)
					controller.SetMessage("Checking for new local matches...");

				var newMatches =
					Helper.MainWindow.DeckList.DecksList.Where(d => d.SyncWithHearthStats.HasValue && d.SyncWithHearthStats.Value)
					      .SelectMany(d => d.DeckStats.Games.Where(g => !g.HasHearthStatsId).Select(g => new {game = g, deck = d}))
					      .ToList();
				if(newGames.Any())
				{
					if(!background)
						controller.SetMessage("Uploading new matches...");
					await Task.Run(() => { Parallel.ForEach(newMatches, match => UploadMatch(match.game, match.deck, false)); });
					DeckStatsList.Save();
				}
				Config.Instance.LastHearthStatsDecksSync = DateTime.Now.ToUnixTime() - 600; //10 minute overlap
				Config.Instance.LastHearthStatsGamesSync = DateTime.Now.ToUnixTime() - 600;
				Config.Save();
				if(!background)
					await controller.CloseAsync();

				Helper.MainWindow.ProgressRingTitleBar.IsActive = false;
				_syncInProgress = false;
			}
			catch(Exception e)
			{
				Logger.WriteLine("There was an error syncing with HearthStats:\n" + e, "HearthStatsManager");
				_syncInProgress = false;
			}
		}

		public static async Task<PostResult> UploadVersionAsync(Deck deck, string hearthStatsId, bool saveFilesAfter = true,
		                                                        bool background = false)
		{
			Logger.WriteLine("trying to upload version " + deck.Version + " of " + deck, "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return PostResult.Failed;
			}
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = true;

			var result = await HearthStatsAPI.PostVersionAsync(deck, hearthStatsId);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to upload version " + deck.Version + " of " + deck, "HearthStatsManager");
				result = await HearthStatsAPI.PostVersionAsync(deck, hearthStatsId);
			}
			if(result.Success && saveFilesAfter)
				Helper.MainWindow.WriteDecks();
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = false;
			if(result.Success)
				Logger.WriteLine("success uploading version " + deck, "HearthStatsManager");
			return result;
		}

		public static PostResult UploadVersion(Deck deck, string hearthStatsId, bool saveFilesAfter = true)
		{
			return UploadVersionAsync(deck, hearthStatsId, saveFilesAfter).Result;
		}

		public static async Task<PostResult> DeleteMatchAsync(GameStats game, bool saveFilesAfter = true, bool background = false)
		{
			Logger.WriteLine("trying to delete game " + game, "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return PostResult.Failed;
			}
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = true;
			var result = await HearthStatsAPI.DeleteMatchAsync(game);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to delete game " + game, "HearthStatsManager");
				result = await HearthStatsAPI.DeleteMatchAsync(game);
			}
			if(result.Success && saveFilesAfter)
				DeckStatsList.Save();
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = false;
			if(result.Success)
				Logger.WriteLine("success deleting game " + game, "HearthStatsManager");
			return result;
		}

		public static async Task<PostResult> MoveMatchAsync(GameStats game, Deck target, bool saveFilesAfter = true, bool background = false)
		{
			Logger.WriteLine("trying to move game " + game + " to " + target, "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return PostResult.Failed;
			}
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = true;
			var result = await HearthStatsAPI.MoveMatchAsync(game, target);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to move game " + game + " to " + target, "HearthStatsManager");
				result = await HearthStatsAPI.MoveMatchAsync(game, target);
			}
			if(result.Success && saveFilesAfter)
				DeckStatsList.Save();
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = false;
			if(result.Success)
				Logger.WriteLine("success moveing game " + game, "HearthStatsManager");
			return result;
		}

		public static async Task<PostResult> UpdateDeckAsync(Deck deck, bool saveFilesAfter = true, bool background = false)
		{
			Logger.WriteLine("trying to update deck " + deck, "HearthStatsManager");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsManager");
				return PostResult.Failed;
			}
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = true;
			var result = await HearthStatsAPI.UpdateDeckAsync(deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Logger.WriteLine("try #2 to update deck " + deck, "HearthStatsManager");
				result = await HearthStatsAPI.UpdateDeckAsync(deck);
			}
			if(result.Success && saveFilesAfter)
				Helper.MainWindow.WriteDecks();
			if(background)
				Helper.MainWindow.ProgressRingTitleBar.IsActive = false;
			if(result.Success)
				Logger.WriteLine("success updating deck " + deck, "HearthStatsManager");
			return result;
		}
	}
}