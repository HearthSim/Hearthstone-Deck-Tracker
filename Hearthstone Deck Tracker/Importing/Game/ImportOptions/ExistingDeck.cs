using System;
using System.Collections.ObjectModel;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Importing.Game.ImportOptions
{
	public class ExistingDeck : IImportOption
	{
		public Deck Deck { get; }

		public string DisplayName => Deck.Name + (NewVersion.Major > 0 ? $" ({NewVersion.ShortVersionString})" : string.Empty);

		public SerializableVersion NewVersion { get; }

		public int MismatchingCards { get; }

		public bool ShouldBeNewDeck { get; }

		public ExistingDeck(Deck deck, HearthMirror.Objects.Deck newDeck)
		{
			Deck = deck;
			var tmp = new Deck
			{
				Cards = new ObservableCollection<Card>(newDeck.Cards.Select(x => new Card(x.Id) { Count = x.Count })),
				Sideboards = newDeck.Sideboards.Select(x => new Sideboard(
					x.Key,
					x.Value.Select(c =>
						{
							var card = Database.GetCardFromId(c.Id);
							if(card == null)
								return null;
							card.Count = c.Count;
							return card;
						}).WhereNotNull().ToList()
				)).ToList()
			};
			MismatchingCards = -1;
			if(deck.HasVersions)
			{
				var counts = deck.VersionsIncludingSelf.Select(v => GetMismatchingCards(tmp, deck.GetVersion(v)));
				if(counts.Any(c => c == 0))
					MismatchingCards = 0;
			}
			if(MismatchingCards == -1)
				MismatchingCards = GetMismatchingCards(tmp, deck);

			NewVersion = MismatchingCards == 0 ? new SerializableVersion(0, 0)
				: (MismatchingCards >= 5 ? SerializableVersion.IncreaseMajor(deck.Version)
					: SerializableVersion.IncreaseMinor(deck.Version));
			ShouldBeNewDeck = MismatchingCards >= 15;
		}

		private int GetMismatchingCards(Deck deck1, Deck deck2)
		{
			var diffMainCards = deck1 - deck2;
			var mismatchingMainCards = Math.Max(diffMainCards.Where(x => x.Count > 0).Sum(x => x.Count),
						diffMainCards.Where(x => x.Count < 0).Sum(x => -x.Count));

			var diffSideboardCards = deck2.GetSideboardDiff(deck1);
			var mismatchingSideboardCards = Math.Max(diffSideboardCards.Where(x => x.Count > 0).Sum(x => x.Count),
				diffSideboardCards.Where(x => x.Count < 0).Sum(x => -x.Count));

			return mismatchingMainCards + mismatchingSideboardCards;
		}
	}
}
