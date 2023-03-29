using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class DeckState
	{
		public DeckState(
			IEnumerable<Card> remainingInDeck,
			IEnumerable<Card> removedFromDeck,
			Dictionary<string, IEnumerable<Card>>? remainingInSideboards = null,
			Dictionary<string, IEnumerable<Card>>? removedFromSideboards = null
		)
		{
			RemovedFromDeck = removedFromDeck;
			RemainingInDeck = remainingInDeck;
			RemovedFromSideboards = removedFromSideboards;
			RemainingInSideboards = remainingInSideboards;
		}

		public IEnumerable<Card> RemainingInDeck { get; }
		public IEnumerable<Card> RemovedFromDeck { get; }

		public Dictionary<string, IEnumerable<Card>>? RemainingInSideboards { get; }
		public Dictionary<string, IEnumerable<Card>>? RemovedFromSideboards { get; }
	}
}
