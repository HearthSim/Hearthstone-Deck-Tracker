using System;
using System.Collections.ObjectModel;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Importing.Game.ImportOptions
{
	public class ExistingDeck : IImportOption
	{
		public Deck Deck { get; }

		public string DisplayName => Deck.Name + (NewVersion.Major > 0 ? $" ({NewVersion.ShortVersionString})" : string.Empty);

		public SerializableVersion NewVersion { get; }

		public int MatchingCards { get; }

		public bool ShouldBeNewDeck { get; }

		public ExistingDeck(Deck deck, HearthMirror.Objects.Deck newDeck)
		{
			Deck = deck;
			var tmp = new Deck { Cards = new ObservableCollection<Card>(newDeck.Cards.Select(x => new Card { Id = x.Id, Count = x.Count })) };
			MatchingCards = 0;
			if(deck.HasVersions)
			{
				var counts = deck.VersionsIncludingSelf.Select(v => GetMatchingCards(tmp, deck.GetVersion(v)));
				if(counts.Any(c => c == 30))
					MatchingCards = 30;
			}
			if(MatchingCards != 30)
				MatchingCards = GetMatchingCards(tmp, deck);
			NewVersion = MatchingCards == 30 ? new SerializableVersion(0, 0)
				: (MatchingCards < 26 ? SerializableVersion.IncreaseMajor(deck.Version)
					: SerializableVersion.IncreaseMinor(deck.Version));
			ShouldBeNewDeck = MatchingCards < 15;
		}

		private int GetMatchingCards(Deck deck1, Deck deck2)
		{
			var diff = deck1 - deck2;
			return 30 - Math.Max(diff.Where(x => x.Count > 0).Sum(x => x.Count),
						diff.Where(x => x.Count < 0).Sum(x => -x.Count));
		}
	}
}
