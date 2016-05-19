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
			MatchingCards = newDeck.Cards.Sum(card => deck.Cards.FirstOrDefault(x => x.Id == card.Id)?.Count ?? 0);
			NewVersion = MatchingCards == 30 ? new SerializableVersion(0, 0)
				: (MatchingCards < 26 ? SerializableVersion.IncreaseMajor(deck.Version)
					: SerializableVersion.IncreaseMinor(deck.Version));
		}
	}
}