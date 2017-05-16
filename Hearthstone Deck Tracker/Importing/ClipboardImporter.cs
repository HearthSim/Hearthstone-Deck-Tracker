using System;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class ClipboardImporter
	{
		public static async Task<Deck> Import()
		{
			try
			{
				Uri uriResult;
				var clipboard = Clipboard.GetText();
				var validUrl = Uri.TryCreate(clipboard, UriKind.Absolute, out uriResult)
								&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
				if(validUrl)
					return await DeckImporter.Import(clipboard);

				var deck = DeckSerializer.Deserialize(clipboard);
				if(deck != null)
					return deck;

				if(StringImporter.IsValidImportString(clipboard))
					return StringImporter.Import(clipboard);

				return JsonDeckImporter.Import(clipboard);

			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}
	}
}
