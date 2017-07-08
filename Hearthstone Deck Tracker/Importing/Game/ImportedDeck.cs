using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing.Game.ImportOptions;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Importing.Game
{
	public class ImportedDeck
	{
		/// <param name="deck">HearthMirror deck object</param>
		/// <param name="matches">Local decks with HsId OR 30 matching cards</param>
		/// <param name="localDecks">All local decks</param>
		public ImportedDeck(HearthMirror.Objects.Deck deck, List<Deck> matches, IList<Deck> localDecks)
		{
			Deck = deck;
			matches = matches ?? new List<Deck>();
			var hero = Database.GetCardFromId(deck.Hero);
			if(string.IsNullOrEmpty(hero?.PlayerClass) || hero.Id == Database.UnknownCardId)
			{
				Log.Error("No hero found for id " + deck.Hero);
				return;
			}
			Class = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(hero.PlayerClass.ToLower());

			var localOptions = localDecks.Where(d => d.Class == Class && !d.Archived && !d.IsArenaDeck)
				.Select(x => new ExistingDeck(x, deck));
			var matchesOptions = matches.Select(x => new ExistingDeck(x, deck)).ToList();
			var importOptions = matchesOptions.Concat(localOptions)
				.GroupBy(x => new {x.Deck.DeckId, x.Deck.Version}).Select(g => g.First())
				.OrderByDescending(x => x.Deck.HsId == deck.Id)
				.ThenByDescending(x => x.MatchingCards)
				.ThenByDescending(x => x.Deck.LastPlayed);

			ImportOptions = New.Concat(importOptions);

			SelectedIndex = matchesOptions.Any(x => !x.ShouldBeNewDeck) ? 1 : 0;
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
