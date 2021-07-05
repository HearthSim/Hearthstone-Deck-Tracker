using System;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Deckstrings;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class ClipboardImporter
	{
		public static async Task<Deck> Import(bool rethrow = false)
		{
			Exception error = null;
			try
			{
				var clipboard = Clipboard.GetText();
				var validUrl = Uri.TryCreate(clipboard, UriKind.Absolute, out Uri uriResult)
								&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
				if(validUrl)
					return await DeckImporter.Import(clipboard);

				try
				{
					var hearthDbDeck = DeckSerializer.Deserialize(clipboard);
					var deck = HearthDbConverter.FromHearthDbDeck(hearthDbDeck);
					if(deck != null)
						return deck;
				}
				catch(Exception e)
				{
					error = e;
					Log.Error(e);
					if(rethrow)
						throw error;
				}

				if(StringImporter.IsValidImportString(clipboard))
					return StringImporter.Import(clipboard);
				return JsonDeckImporter.Import(clipboard);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				if(rethrow)
					throw error ?? ex;
				return null;
			}
		}
	}
}
