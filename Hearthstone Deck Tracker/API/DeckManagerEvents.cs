#region

using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.API
{
	public class DeckManagerEvents
	{
		public static readonly ActionList<Deck> OnDeckUpdated = new ActionList<Deck>();
		public static readonly ActionList<Deck> OnDeckCreated = new ActionList<Deck>();
		public static readonly ActionList<Deck> OnDeckSelected = new ActionList<Deck>();
		public static readonly ActionList<IEnumerable<Deck>> OnDeckDeleted = new ActionList<IEnumerable<Deck>>();
	}
}
