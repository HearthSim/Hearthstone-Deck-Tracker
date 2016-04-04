#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	public class HearthStatsManager
	{
		public const int RetryDelay = 5000;
		public const int VersionDelay = 1000;
		public const int SyncOffset = 600;
		private static int _backgroundActivities;
		public static bool SyncInProgress { get; private set; }

		public static TimeSpan TimeSinceLastSync
			=> DateTime.Now.Subtract(Helper.FromUnixTime(Config.Instance.LastHearthStatsGamesSync + SyncOffset));

		private static void AddBackgroundActivity()
		{
			_backgroundActivities++;
			if(!Core.MainWindow.ProgressRingTitleBar.IsActive)
				Log.Info("background process indicator enabled");
			Core.MainWindow.ProgressRingTitleBar.IsActive = true;
		}

		private static void RemoveBackgroundActivity()
		{
			_backgroundActivities--;
			if(_backgroundActivities <= 0)
			{
				_backgroundActivities = 0;
				if(Core.MainWindow.ProgressRingTitleBar.IsActive)
					Log.Info("background process indicator disabled");
				Core.MainWindow.ProgressRingTitleBar.IsActive = false;
			}
		}

		public static async Task<PostResult> UploadMatchAsync(GameStats game, Deck deck, bool saveFilesAfter = true, bool background = false)
		{
			Log.Info("trying to upload match: " + game);
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("not logged in");
				return PostResult.Failed;
			}
			if(!HearthStatsAPI.IsValidGame(game))
				return PostResult.Failed;
			if(background)
				AddBackgroundActivity();
			if(!deck.HasHearthStatsId)
			{
				Log.Info("...deck has no HearthStats id, uploading deck");
				var success = await UploadDeckAsync(deck);
				if(!success.Success)
				{
					Log.Error("deck could not be uploaded or did not return an id. Can not upload match.");
					if(background)
						RemoveBackgroundActivity();
					return PostResult.Failed;
				}
			}
			var result = await HearthStatsAPI.PostGameResultAsync(game, deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Log.Info("try #2 to upload match: " + game);
				result = await HearthStatsAPI.PostGameResultAsync(game, deck);
			}
			if(result.Success && saveFilesAfter)
				DeckStatsList.Save();
			if(background)
				RemoveBackgroundActivity();
			if(result.Success)
				Log.Info("success uploading match " + game);
			return result;
		}

		public static async Task<PostResult> UploadMultipleMatchesAsync(IEnumerable<GameStats> games, Deck deck, bool saveFilesAfter = true,
		                                                                bool background = false)
		{
			Log.Info($"trying to upload {games.Count()} matches for deck {deck.Name}");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("error: not logged in");
				return PostResult.Failed;
			}
			List<GameStats> validGames = games.Where(HearthStatsAPI.IsValidGame).ToList();
			if(!validGames.Any())
			{
				Log.Error("No valid games");
				return PostResult.Failed;
			}
			if(background)
				AddBackgroundActivity();
			if(!deck.HasHearthStatsId)
			{
				Log.Info("...deck has no HearthStats id, uploading deck");
				var success = await UploadDeckAsync(deck);
				if(!success.Success)
				{
					Log.Error("deck could not be uploaded or did not return an id. Can not upload match.");
					if(background)
						RemoveBackgroundActivity();
					return PostResult.Failed;
				}
			}
			var result = await HearthStatsAPI.PostMultipleGameResultsAsync(validGames, deck);
			if(result.Success && saveFilesAfter)
				DeckStatsList.Save();
			if(background)
				RemoveBackgroundActivity();
			if(result.Success)
				Log.Info($"success uploading {validGames.Count} matches");
			return result;
		}

		public static async Task<PostResult> UploadArenaMatchAsync(GameStats game, Deck deck, bool saveFilesAfter = false,
		                                                           bool background = false)
		{
			Log.Info("trying to upload arena match: " + game);
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("error: not logged in");
				return PostResult.Failed;
			}
			if(background)
				AddBackgroundActivity();
			if(!deck.HasHearthStatsArenaId)
			{
				Log.Info("...deck has no HearthStatsArenaId, creating arena run");
				var createRun = await CreateArenaRunAsync(deck, false, background);
				if(!createRun.Success)
				{
					Log.Error("could not create arena run.");
					if(background)
						RemoveBackgroundActivity();
					return PostResult.Failed;
				}
			}
			var result = await HearthStatsAPI.PostArenaMatch(game, deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Log.Info("try #2 to upload arena match: " + game);
				result = await HearthStatsAPI.PostArenaMatch(game, deck);
			}
			if(result.Success && saveFilesAfter)
				DeckStatsList.Save();
			if(background)
				RemoveBackgroundActivity();
			if(result.Success)
				Log.Info("success uploading arena match " + game);
			return result;
		}

		public static async Task<PostResult> CreateArenaRunAsync(Deck deck, bool saveFilesAfter = false, bool background = false)
		{
			Log.Info("trying to create arena run for deck " + deck);
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("not logged in");
				return PostResult.Failed;
			}
			if(background)
				AddBackgroundActivity();

			var result = await HearthStatsAPI.CreatArenaRunAsync(deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Log.Info("try #2 to create arena run for deck " + deck);
				result = await HearthStatsAPI.CreatArenaRunAsync(deck);
			}
			if(result.Success)
			{
				if(saveFilesAfter)
					DeckList.Save();
				if(background)
					RemoveBackgroundActivity();
				Log.Info("success uploading deck " + deck);
				return PostResult.WasSuccess;
			}
			if(background)
				RemoveBackgroundActivity();
			return PostResult.Failed;
		}

		public static PostResult UploadMatch(GameStats game, Deck deck, bool saveFilesAfter = true)
		{
			return UploadMatchAsync(game, deck, saveFilesAfter).Result;
		}

		public static PostResult UploadMultipleMatches(IEnumerable<GameStats> games, Deck deck, bool saveFilesAfter = true)
		{
			return UploadMultipleMatchesAsync(games, deck, saveFilesAfter).Result;
		}

		public static async Task<List<Deck>> DownloadDecksAsync(bool forceAll = false)
		{
			Log.Info("trying to download decks");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("not logged in");
				return null;
			}
			var decks = await HearthStatsAPI.GetDecksAsync(forceAll ? 0 : Config.Instance.LastHearthStatsDecksSync);
			if(decks == null || decks.Count == 0)
			{
				Log.Info("no new decks");
				return new List<Deck>();
			}
			return decks;
		}

		public static async Task<List<GameStats>> DownloadGamesAsync(bool forceAll = false)
		{
			Log.Info("trying to download games");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("not logged in");
				return null;
			}
			var games = await HearthStatsAPI.GetGamesAsync(forceAll ? 0 : Config.Instance.LastHearthStatsGamesSync);
			if(games == null || games.Count == 0)
			{
				Log.Info("no new games");
				return new List<GameStats>();
			}
			return games;
		}

		public static async Task<bool> DeleteDeckAsync(Deck deck, bool saveFilesAfter = true, bool background = false)
		{
			return await DeleteDeckAsync(new[] {deck}, saveFilesAfter, background);
		}

		public static async Task<bool> DeleteDeckAsync(IEnumerable<Deck> decks, bool saveFilesAfter = true, bool background = false)
		{
			var deckNames = decks.Select(d => d.Name).Aggregate((c, n) => c + ", " + n);
			Log.Info("trying to delete decks: " + deckNames);
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("not logged in");
				return false;
			}
			if(background)
				AddBackgroundActivity();
			var result = await HearthStatsAPI.DeleteDeckAsync(decks);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Log.Info("try #2 to delete decks " + deckNames);
				result = await HearthStatsAPI.DeleteDeckAsync(decks);
			}
			if(result.Success)
			{
				Log.Info("success deleting decks " + deckNames);
				if(saveFilesAfter)
					DeckList.Save();
				if(background)
					RemoveBackgroundActivity();
			}
			if(background)
				RemoveBackgroundActivity();
			return result.Success;
		}

		public static async Task<PostResult> UploadDeckAsync(Deck deck, bool saveFilesAfter = true, bool background = false)
		{
			Log.Info("trying to upload deck " + deck);
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("not logged in");
				return PostResult.Failed;
			}

			if(background)
				AddBackgroundActivity();
			var first = deck.GetVersion(1, 0);
			if(!first.IsArenaDeck && first.HasHearthStatsId && !deck.HasHearthStatsId && !deck.HearthStatsIdsAlreadyReset)
			{
				first.HearthStatsId = first.HearthStatsIdForUploading;
				await HearthStatsAPI.DeleteDeckAsync(first);
				await Task.Delay(1000);

				//reset everything
				foreach(var version in deck.VersionsIncludingSelf.Select(deck.GetVersion))
				{
					version.ResetHearthstatsIds();
					foreach(var game in version.DeckStats.Games)
					{
						game.HearthStatsDeckId = null;
						game.HearthStatsDeckVersionId = null;
						game.HearthStatsId = null;
					}
				}
			}
			var result = await HearthStatsAPI.PostDeckAsync(first, deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Log.Info("try #2 to upload deck " + deck);
				result = await HearthStatsAPI.PostDeckAsync(first, deck);
			}
			if(result.Success)
			{
				var versions =
					deck.VersionsIncludingSelf.Where(v => v != new SerializableVersion(1, 0))
					    .Select(deck.GetVersion)
					    .Where(d => d != null && !d.HasHearthStatsDeckVersionId)
					    .ToList();
				if(versions.Any())
				{
					foreach(var v in versions)
					{
						await Task.Delay(VersionDelay);
						await UploadVersionAsync(v, first.HearthStatsIdForUploading, false);
					}
					deck.HearthStatsId = first.HearthStatsId;
					first.HearthStatsId = "";
					first.HearthStatsIdForUploading = deck.HearthStatsId;
				}
				if(saveFilesAfter)
					DeckList.Save();
				if(background)
					RemoveBackgroundActivity();
				Log.Info("success uploading deck " + deck);
				return PostResult.WasSuccess;
			}
			if(background)
				RemoveBackgroundActivity();
			return PostResult.Failed;
		}

		public static PostResult UploadDeck(Deck deck, bool saveFilesAfter = true)
		{
			return UploadDeckAsync(deck, saveFilesAfter).Result;
		}

		public static async void SyncAsync(bool forceFullSync = false, bool background = false)
		{
			Log.Info($"starting sync process: forceFullSync={forceFullSync}, background={background}");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("not logged in");
				return;
			}
			try
			{
				if(SyncInProgress)
				{
					Log.Error("error: sync already in progress");
					return;
				}
				SyncInProgress = true;
				if(background)
					AddBackgroundActivity();

				var controller = background ? null : await Core.MainWindow.ShowProgressAsync("Syncing...", "Checking HearthStats for new decks...");
				Log.Info("Checking HearthStats for new decks...");
				var localDecks = DeckList.Instance.Decks;
				var remoteDecks = await DownloadDecksAsync(forceFullSync);
				if(remoteDecks.Any())
				{
					var newDecks = remoteDecks.Where(deck => localDecks.All(localDeck => localDeck.HearthStatsId != deck.HearthStatsId)).ToList();
					if(newDecks.Any())
					{
						Core.MainWindow.FlyoutHearthStatsDownload.IsOpen = true;
						if(!background)
							await controller.CloseAsync();
						newDecks = await Core.MainWindow.HearthStatsDownloadDecksControl.LoadDecks(newDecks);
						foreach(var deck in newDecks)
						{
							DeckList.Instance.Decks.Add(deck);
							Log.Info("saved new deck " + deck);
						}
						DeckList.Save();
						Core.MainWindow.DeckPickerList.UpdateDecks();
						Core.MainWindow.DeckPickerList.UpdateArchivedClassVisibility();
						background = false;
					}
					if(!background)
					{
						if(controller == null || !controller.IsOpen)
							controller = await Core.MainWindow.ShowProgressAsync("Syncing...", "Checking for new versions...");
						else
							controller.SetMessage("Checking for new versions...");
					}
					Log.Info("Checking for new versions...");
					var decksWithNewVersions =
						remoteDecks.Where(
						                  deck =>
						                  localDecks.Any(
						                                 localDeck =>
						                                 localDeck.HasHearthStatsId && deck.HearthStatsId == localDeck.HearthStatsId
						                                 && localDeck.GetMaxVerion() != deck.GetMaxVerion())).ToList();
					if(decksWithNewVersions.Any())
					{
						foreach(var deck in decksWithNewVersions)
						{
							var currentDeck = localDecks.FirstOrDefault(d => d.HasHearthStatsId && d.HearthStatsId == deck.HearthStatsId);
							if(currentDeck == null)
								continue;
							var versions =
								deck.VersionsIncludingSelf.Where(v => !currentDeck.VersionsIncludingSelf.Contains(v))
								    .OrderBy(v => v)
								    .Select(v => deck.GetVersion(v))
								    .Where(v => v != null)
								    .ToList();
							if(versions.Any())
							{
								foreach(var newVersion in versions)
								{
									var clone = (Deck)currentDeck.Clone();
									currentDeck.Version = newVersion.Version;
									currentDeck.SelectedVersion = newVersion.Version;
									currentDeck.HearthStatsDeckVersionId = newVersion.HearthStatsDeckVersionId;
									currentDeck.Versions.Add(clone);
								}
								Log.Info($"saved {versions.Count} new versions ({versions.Select(v => v.Version.ToString()).Aggregate((c, n) => c + ", " + n)}) to {deck}");
							}
						}
						DeckList.Save();
						Core.MainWindow.DeckPickerList.UpdateDecks();
						Core.MainWindow.DeckPickerList.UpdateArchivedClassVisibility();
					}
					if(!background)
					{
						if(controller == null || !controller.IsOpen)
							controller = await Core.MainWindow.ShowProgressAsync("Syncing...", "Checking for edited decks...");
						else
							controller.SetMessage("Checking for edited decks...");
					}
					Log.Info("Checking for edited decks...");
					var editedDecks =
						remoteDecks.Where(
						                  r =>
						                  localDecks.Any(
						                                 l =>
						                                 l.HasHearthStatsId && l.HearthStatsId == r.HearthStatsId && r.LastEdited > l.LastEdited
						                                 && (l.Name != r.Name || !(new HashSet<string>(l.Tags).SetEquals(r.Tags)) || l.Note != r.Note
						                                     || (l - r).Count > 0))).ToList();
					if(editedDecks.Any())
					{
						foreach(var deck in editedDecks)
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
								Log.Info("edited latest version of " + localDeck);
							}
						}
						Core.MainWindow.DeckPickerList.UpdateDecks();
						Core.MainWindow.DeckPickerList.UpdateArchivedClassVisibility();
						DeckList.Save();
						Log.Info($"edited {editedDecks.Count} decks");
					}
				}

				if(!background)
				{
					if(controller == null || !controller.IsOpen)
						controller = await Core.MainWindow.ShowProgressAsync("Syncing...", "Checking HearthStats for new matches...");
					else
						controller.SetMessage("Checking HearthStats for new matches...");
				}
				Log.Info("Checking HearthStats for new matches...");
				var newGames = await DownloadGamesAsync(forceFullSync);
				if(newGames.Any())
				{
					foreach(var game in newGames)
					{
						var deck =
							DeckList.Instance.Decks.FirstOrDefault(
							                                       d =>
							                                       d.VersionsIncludingSelf.Select(v => d.GetVersion(v.Major, v.Minor))
							                                        .Where(v => v != null && v.HasHearthStatsDeckVersionId)
							                                        .Any(v => game.HearthStatsDeckVersionId == v.HearthStatsDeckVersionId));
						if(deck == null)
						{
							//deck_version_id seems to be null for older matches
							deck = DeckList.Instance.Decks.FirstOrDefault(d => d.HasHearthStatsId && game.HearthStatsDeckId == d.HearthStatsId);
							if(deck != null)
								game.HearthStatsDeckVersionId = deck.HearthStatsDeckVersionId;
						}
						if(deck == null)
						{
							Log.Warn($"no deck found for match {game}");
							continue;
						}
						if(deck.DeckStats.Games.Any(g => g.HearthStatsId == game.HearthStatsId))
						{
							Log.Warn($"deck {deck} already has match {game}");
							continue;
						}
						var deckVersion =
							deck.VersionsIncludingSelf.Select(deck.GetVersion)
							    .FirstOrDefault(v => v.HearthStatsDeckVersionId == game.HearthStatsDeckVersionId);
						if(deckVersion == null)
							continue;
						Log.Info($"added match {game} to version {deck.Version.ShortVersionString} of deck {deck}");
						game.PlayerDeckVersion = deckVersion.Version;
						deck.DeckStats.AddGameResult(game);
					}
					DeckStatsList.Save();
					Core.MainWindow.DeckPickerList.UpdateDecks();
					Core.MainWindow.DeckPickerList.UpdateArchivedClassVisibility();
					Core.MainWindow.DeckStatsFlyout.LoadOverallStats();
				}

				if(!background)
					controller.SetMessage("Checking for new local decks...");
				Log.Info("Checking for new local decks...");
				var newLocalDecks = localDecks.Where(deck => !deck.HasHearthStatsId && deck.IsArenaDeck != true).ToList();
				if(newLocalDecks.Any(d => d.SyncWithHearthStats != false))
				{
					var uploaded = 0;
					var total = newLocalDecks.Count;
					Log.Info("found " + newLocalDecks.Count + " new decks");
					if(!background)
						await controller.CloseAsync();
					Core.MainWindow.FlyoutHearthStatsUpload.IsOpen = true;
					newLocalDecks = await Core.MainWindow.HearthStatsUploadDecksControl.LoadDecks(newLocalDecks);
					if(newLocalDecks.Any())
					{
						controller = await Core.MainWindow.ShowProgressAsync("Syncing...", "Uploading " + newLocalDecks.Count + " new decks...");
						Log.Info("Uploading " + newLocalDecks.Count + " new decks...");
						await Task.Run(() =>
						{
							Parallel.ForEach(newLocalDecks, deck =>
							{
								UploadDeck(deck, false);

								if(controller != null)
									Core.MainWindow.Dispatcher.BeginInvoke(new Action(() => { controller.SetProgress(1.0 * (++uploaded) / total); }));
							});
						});
						DeckList.Save(); //save new ids
						background = false;
					}
				}

				if(!background)
				{
					if(controller == null || !controller.IsOpen)
						controller = await Core.MainWindow.ShowProgressAsync("Syncing...", "Checking for new local versions...");
					else
						controller.SetMessage("Checking for new local versions...");
				}

				Log.Info("Checking for new local versions...");
				var localNewVersions =
					localDecks.Where(x => x.HasHearthStatsId)
					          .SelectMany(
					                      x =>
					                      x.VersionsIncludingSelf.Select(x.GetVersion)
					                       .Where(v => !v.HasHearthStatsDeckVersionId)
					                       .Select(v => new {version = v, hearthStatsId = x.HearthStatsIdForUploading}))
					          .ToList();
				if(localNewVersions.Any())
				{
					var uploaded = 0;
					var total = localNewVersions.Count;
					if(!background)
						controller.SetMessage("Uploading " + localNewVersions.Count + " new versions...");
					Log.Info("Uploading " + localNewVersions.Count + " new versions...");
					//this can't happen in parallel (?)
					foreach(var v in localNewVersions)
					{
						var result = await UploadVersionAsync(v.version, v.hearthStatsId, false);
						if(!result.Success && result.Retry)
						{
							await Task.Delay(RetryDelay);
							await UploadVersionAsync(v.version, v.hearthStatsId, false);

							if(controller != null)
								Core.MainWindow.Dispatcher.BeginInvoke(new Action(() => { controller.SetProgress(1.0 * (++uploaded) / total); })).Task.Forget();
						}
					}
					DeckList.Save();
				}
				if(!background)
					controller.SetMessage("Checking for edited local decks...");
				Log.Info("Checking for edited local decks...");

				var editedLocalDecks =
					localDecks.Where(
					                 l =>
					                 remoteDecks.Any(
					                                 r =>
					                                 r.HasHearthStatsId && r.HearthStatsId == l.HearthStatsId && l.LastEdited > r.LastEdited
					                                 && (r.Name != l.Name || !(new HashSet<string>(r.Tags).SetEquals(l.Tags)) || r.Note != l.Note
					                                     || (r - l).Count > 0))).ToList();
				if(editedLocalDecks.Any())
				{
					if(!background)
						controller.SetMessage("Updating " + editedLocalDecks.Count + " decks...");
					Log.Info("Updating " + editedLocalDecks.Count + " decks...");
					foreach(var deck in editedLocalDecks)
						await UpdateDeckAsync(deck);
					Log.Info("updated " + editedLocalDecks.Count + " decks");
				}

				if(!background)
					controller.SetMessage("Checking for new local matches...");
				Log.Info("Checking for new local matches...");

				var newMatches =
					DeckList.Instance.Decks.Where(d => d.SyncWithHearthStats == true && d.HasHearthStatsId)
					        .SelectMany(d => d.DeckStats.Games.Where(g => !g.HasHearthStatsId).Select(g => new {deck = d, game = g}))
					        .ToList();
				if(newMatches.Any())
				{
					var uploaded = 0;
					var total = newMatches.Count;
					if(!background)
						controller.SetMessage("Uploading " + newMatches.Count + " new matches...");
					Log.Info("Uploading " + newMatches.Count + " new matches...");
					await Task.Run(() =>
					{
						var groupedMatchObs =
							newMatches.Select(
							                  match =>
							                  new
							                  {
								                  match.game,
								                  deckVersion =
								                  (match.game.HasHearthStatsDeckVersionId
									                   ? (match.deck.VersionsIncludingSelf.Where(v => v != null)
									                           .Select(match.deck.GetVersion)
									                           .FirstOrDefault(
									                                           d =>
									                                           d.HasHearthStatsDeckVersionId
									                                           && d.HearthStatsDeckVersionId == match.game.HearthStatsDeckVersionId)
									                      ?? match.deck.GetVersion(match.game.PlayerDeckVersion))
									                   : (match.game.PlayerDeckVersion != null ? match.deck.GetVersion(match.game.PlayerDeckVersion) : match.deck))
							                  }).Where(x => x.deckVersion != null).GroupBy(x => x.deckVersion.HearthStatsDeckVersionId);

						Parallel.ForEach(groupedMatchObs, matches =>
						{
							UploadMultipleMatches(matches.Select(m => m.game), matches.First().deckVersion, false);
							if(controller != null)
							{
								Core.MainWindow.Dispatcher.BeginInvoke(
								                                       new Action(
									                                       () => { controller.SetProgress(1.0 * (uploaded += matches.Count()) / total); }));
							}
						});
					});
					DeckStatsList.Save();
				}
				Config.Instance.LastHearthStatsDecksSync = DateTime.Now.ToUnixTime() - SyncOffset; //10 minute overlap
				Config.Instance.LastHearthStatsGamesSync = DateTime.Now.ToUnixTime() - SyncOffset;
				Config.Save();
				if(!background)
					await controller.CloseAsync();

				RemoveBackgroundActivity();
				SyncInProgress = false;
				Log.Info("finished sync process");
			}
			catch(Exception e)
			{
				Log.Error("There was an error syncing with HearthStats:\n" + e);
				SyncInProgress = false;
			}
		}

		public static async Task<PostResult> UploadVersionAsync(Deck deck, string hearthStatsId, bool saveFilesAfter = true,
		                                                        bool background = false)
		{
			Log.Info("trying to upload version " + deck.Version + " of " + deck);
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("not logged in");
				return PostResult.Failed;
			}
			if(background)
				AddBackgroundActivity();

			var result = await HearthStatsAPI.PostVersionAsync(deck, hearthStatsId);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Log.Info("try #2 to upload version " + deck.Version + " of " + deck);
				result = await HearthStatsAPI.PostVersionAsync(deck, hearthStatsId);
			}
			if(result.Success && saveFilesAfter)
				DeckList.Save();
			if(background)
				RemoveBackgroundActivity();
			if(result.Success)
				Log.Info("success uploading version " + deck);
			return result;
		}

		public static PostResult UploadVersion(Deck deck, string hearthStatsId, bool saveFilesAfter = true)
		{
			return UploadVersionAsync(deck, hearthStatsId, saveFilesAfter).Result;
		}

		public static async Task<PostResult> DeleteMatchesAsync(List<GameStats> games, bool saveFilesAfter = true, bool background = false)
		{
			Log.Info("trying to delete game " + games.Select(g => g.ToString()).Aggregate((c, n) => c + ", " + n));
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("not logged in");
				return PostResult.Failed;
			}
			if(background)
				AddBackgroundActivity();
			var result = await HearthStatsAPI.DeleteMatchesAsync(games);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Log.Info("try #2 to delete game " + games);
				result = await HearthStatsAPI.DeleteMatchesAsync(games);
			}
			if(result.Success && saveFilesAfter)
				DeckStatsList.Save();
			if(background)
				RemoveBackgroundActivity();
			if(result.Success)
				Log.Info("success deleting game " + games);
			return result;
		}

		public static async Task<PostResult> MoveMatchAsync(GameStats game, Deck target, bool saveFilesAfter = true, bool background = false)
		{
			Log.Info("trying to move game " + game + " to " + target);
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("error: not logged in");
				return PostResult.Failed;
			}
			if(background)
				AddBackgroundActivity();
			var result = await HearthStatsAPI.MoveMatchAsync(game, target);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Log.Info("try #2 to move game " + game + " to " + target);
				result = await HearthStatsAPI.MoveMatchAsync(game, target);
			}
			if(result.Success && saveFilesAfter)
				DeckStatsList.Save();
			if(background)
				RemoveBackgroundActivity();
			if(result.Success)
				Log.Info("success moveing game " + game);
			return result;
		}

		public static async Task<PostResult> UpdateDeckAsync(Deck deck, bool saveFilesAfter = true, bool background = false)
		{
			Log.Info("trying to update deck " + deck);
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Log.Error("error: not logged in");
				return PostResult.Failed;
			}
			if(background)
				AddBackgroundActivity();
			var result = await HearthStatsAPI.UpdateDeckAsync(deck);
			if(!result.Success && result.Retry)
			{
				await Task.Delay(RetryDelay);
				Log.Info("try #2 to update deck " + deck);
				result = await HearthStatsAPI.UpdateDeckAsync(deck);
			}
			if(result.Success && saveFilesAfter)
				DeckList.Save();
			if(background)
				RemoveBackgroundActivity();
			if(result.Success)
				Log.Info("success updating deck " + deck);
			return result;
		}

		public static async void UpdateArenaMatchAsync(GameStats game, Deck deck, bool saveFilesAfter = true, bool background = false)
		{
			var result = await DeleteMatchesAsync(new List<GameStats> {game}, saveFilesAfter, background);
			if(result.Success)
			{
				game.ResetHearthstatsIds();
				await UploadArenaMatchAsync(game, deck, saveFilesAfter, background);
			}
		}

		public static async void UpdateMatchAsync(GameStats game, Deck deck, bool saveFilesAfter = true, bool background = false)
		{
			var result = await DeleteMatchesAsync(new List<GameStats> {game}, saveFilesAfter, background);
			if(result.Success)
			{
				game.ResetHearthstatsIds();
				await UploadMatchAsync(game, deck, saveFilesAfter, background);
			}
		}
	}
}