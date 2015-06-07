#region

using System;
using System.Linq;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.HearthStats.API;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Protocol
{
	[JsonObject]
	public class JsonDecksWrapper
	{
		[JsonProperty("autosave")]
		public bool AutoSave = false;

		[JsonProperty("decks")]
		public JsonDeck[] Decks = {};

		public async void SaveDecks()
		{
			var tags = Decks.Where(d => d.Tags != null).SelectMany(x => x.Tags).ToList();
			if(tags.Any())
			{
				var reloadTags = false;
				foreach(var tag in tags)
				{
					if(!DeckList.Instance.AllTags.Contains(tag))
					{
						DeckList.Instance.AllTags.Add(tag);
						reloadTags = true;
					}
				}
				if(reloadTags)
				{
					DeckList.Save();
					Helper.MainWindow.ReloadTags();
				}
			}

			var decks = Decks.Select(x => x.ToDeck()).ToList();

			Helper.MainWindow.ActivateWindow();
			if(decks.Count > 1 || AutoSave)
			{
				var log = string.Format("Saving {0} decks", decks.Count);
				var controller = await Helper.MainWindow.ShowProgressAsync(log, "Please wait...");
				Logger.WriteLine(log);
				foreach(var deck in decks)
				{
					try
					{
						DeckList.Instance.Decks.Add(deck);
						if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
						{
							Logger.WriteLine("auto uploading new deck", "JsonDecksWrapper");
							await HearthStatsManager.UploadDeckAsync(deck, background: true);
						}
						DeckManagerEvents.OnDeckCreated.Execute(deck);
					}
					catch(Exception ex)
					{
						Logger.WriteLine("Error saving deck: " + ex, "JsonDecksWrapper");
					}
				}
				Helper.MainWindow.DeckPickerList.UpdateDecks();
				await controller.CloseAsync();
			}
			else if(decks.Count == 1)
				Helper.MainWindow.SetNewDeck(decks[0]);
		}
	}
}