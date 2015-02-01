using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	class HearthStatsSync
	{
		private DateTime lastCheckDate;

		public HearthStatsSync()
		{
			
		}
		public void Sync()
		{
			var remoteDecks = GetDecks(lastCheckDate);
			var localDecks = GetLocalDecks();

			foreach(var deck in localDecks.Where(d => d.SyncWithHearthStats.HasValue))
			{
				if(deck.HasHearthStatsId)
				{

					if(remoteDecks.Any(d => d.HearthStatsId == deck.HearthStatsId))
					{
						//server has deck with same id
						var result = CompareDecks();
						if(result == DeckComparisonResult.LocalIsNewer)
							_uploadQueue.Enqueue(deck);
						else if(result == DeckComparisonResult.RemoteIsNewer)
							_downloadQueue.Enqueue(deck);
						remoteDecks.Remove(deck);
					}
					else
						//server does not have deck with id
						_uploadOrDeleteQueue.Enqueue(deck);
				}
				else
				{
					//check is server has the deck (local is missing id)
					//var deckOnRemote = remoteDecks.FirstOrDefault(d => d.Name == deck.Name && d.Versions.All(v => deck.Versi.Any(d => d.Cards == v.Cards)))
			       // if(existsOnRemote != null)
					//	deck.id = existsOnRemote.id;
					//else
						_uploadOrDeleteQueue.Enqueue(deck);
				}
			}
			//download remaining decks
			foreach(var deck in remoteDecks)
				_downloadQueue.Enqueue(deck);
		}

		private Queue _uploadQueue;
		private Queue _uploadOrDeleteQueue;
		private Queue _downloadQueue;

		private DeckComparisonResult CompareDecks()
		{
			throw new NotImplementedException();
		}

		private List<Deck> GetLocalDecks()
		{
			throw new NotImplementedException();
		}

		private List<Deck> GetDecks(DateTime dateTime)
		{
			throw new NotImplementedException();
		}

		private enum DeckComparisonResult
		{
			Equal,
            LocalIsNewer,
			RemoteIsNewer
		}

		public static async Task<bool> UploadMatch(GameStats game, Deck deck, bool saveFilesAfter = true)
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
				var success = await HearthStatsAPI.PostDeck(deck);
				if(!success)
				{
					Logger.WriteLine("error: deck could not be uploaded or did not return an id. Can not upload match.", "HearthStatsSync");
					return false;
				}
			}
			await HearthStatsAPI.PostGameResult(game, deck);
			Logger.WriteLine("upload complete", "HearthStatsSync");
			if(saveFilesAfter)
			{
				DeckStatsList.Save();
				Helper.MainWindow.WriteDecks();
			}
			return true;
		}

		public static async Task<List<Deck>> DownloadDecks(bool forceAll = false)
		{
			Logger.WriteLine("trying do download decks", "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return null;
			}
			var json = await HearthStatsAPI.GetDecks(forceAll ? 0 : Config.Instance.LastHearthStatsDecksSync);
			Config.Instance.LastHearthStatsDecksSync = DateTime.Now.ToUnixTime();
			Config.Save();

			//convert json to deck list

			return new List<Deck>();

		}

		public static async Task<List<GameStats>> DownloadGames(bool forceAll = false)
		{
			Logger.WriteLine("trying do download decks", "HearthStatsSync");
			if(!HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("error: not logged in", "HearthStatsSync");
				return null;
			}
			var json = await HearthStatsAPI.GetGames(forceAll ? 0 : Config.Instance.LastHearthStatsGamesSync);
			Config.Instance.LastHearthStatsGamesSync = DateTime.Now.ToUnixTime();
			Config.Save();

			var gameObjs = (dynamic[])JsonConvert.DeserializeObject(json);
			var games =
				gameObjs.Select(
				                x =>
				                new GameStats()
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

		public static async Task<bool> DeleteDeck(Deck deck)
		{
			return await HearthStatsAPI.DeleteDeck(deck);
		}
	}
}
