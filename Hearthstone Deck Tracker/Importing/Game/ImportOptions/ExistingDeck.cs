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

		public ExistingDeck(Deck deck, HearthMirror.Objects.Deck newDeck)
		{
			Deck = deck;
			var tmp = new Deck { Cards = new ObservableCollection<Card>(newDeck.Cards.Select(x => new Card { Id = x.Id, Count = x.Count })) };
			MatchingCards = 30 - (deck - tmp).Count(x => x.Count > 0);
			NewVersion = MatchingCards == 30 ? new SerializableVersion(0, 0)
				: (MatchingCards < 26 ? SerializableVersion.IncreaseMajor(deck.Version)
					: SerializableVersion.IncreaseMinor(deck.Version));
		}
	}
}