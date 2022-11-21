
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments
{
	internal class ValueMoment
	{
		public abstract class VMName {
			public const string CopyDeck = "Copy Deck";
			public const string ShareDeck = "Share Deck";
			public const string PersonalStats = "Review Stats About My Progress/Performance";

			// HS-Constructed
			public const string DecklistVisible = "Overlay Decklist Visible";

			// Battlegrounds
			public const string BGBobsBuddy = "Overlay Bob's Buddy";
			public const string BGSessionRecap = "Overlay Session Recap";
			public const string BGMinionBrowser = "Explore Minion Browser Tabs";

			// Mercenaries
			public const string MercOpponentAbilities = "Hover Opponent Merc Abilities";
			public const string MercMyTasks = "Hover My Merc Tasks";
		}

		public enum VMKind
		{
			Free,
			Paid,
		}

		public ValueMoment(string name, VMKind kind, int maxValueMomentCount = 100)
		{
			Name = name;
			Kind = kind;
			MaxValueMomentCount = maxValueMomentCount;
		}

		public string Name { get; }

		public VMKind Kind { get; }

		public int MaxValueMomentCount { get; }

		public bool IsFree => Kind == VMKind.Free;

		public bool IsPaid => Kind == VMKind.Paid;
	}
}
