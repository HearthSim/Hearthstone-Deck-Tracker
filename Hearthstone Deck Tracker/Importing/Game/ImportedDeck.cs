using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing.Game.ImportOptions;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Importing.Game
{
	public class ImportedDeck
	{
		public ImportedDeck(HearthMirror.Objects.Deck deck, List<Deck> candidates)
		{
			Deck = deck;
			candidates = candidates ?? new List<Deck>();
			var hero = HearthDb.Cards.All[deck.Hero];
			Class = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(hero.Class.ToString().ToLower());
			ImportOptions =
				New.Concat(candidates.Concat(DeckList.Instance.Decks.Where(x => x.Class == Class && !x.Archived && !x.IsArenaDeck)).Distinct()
					.Select(x => new ExistingDeck(x, deck)).OrderByDescending(x => x.MatchingCards).ThenByDescending(x => x.Deck.LastPlayed));
			SelectedIndex = candidates.Any() ? 1 : 0;
		}

		public HearthMirror.Objects.Deck Deck { get; }
		public string Class { get; }
		public bool Import { get; set; } = true;
		public IEnumerable<IImportOption> ImportOptions { get; }
		public int SelectedIndex { get; set; }

		public IImportOption SelectedImportOption => ImportOptions.ElementAt(SelectedIndex);
		public BitmapImage ClassImage => ImageCache.GetClassIcon(Class);
		private IEnumerable<IImportOption> New => new IImportOption[] { new NewDeck() };
	}
}