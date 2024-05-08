
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments
{
	public class ValueMoment
	{
		public abstract class VMName {
			public const string CopyDeck = "Copy Deck";
			public const string ShareDeck = "Share Deck";
			public const string PersonalStats = "Review Stats About My Progress/Performance";

			// HS-Constructed
			public const string HSDecklistVisible = "Overlay Decklist Visible";
			public const string HSMulliganGuideOverlay = "Support Mulligan Choice";

			// Battlegrounds
			public const string BGBobsBuddy = "Overlay Bob's Buddy";
			public const string BGSessionRecap = "Overlay Session Recap";
			public const string BGMinionBrowser = "Explore Minion Browser Tabs";
			public const string BGMinionBrowserMinionType = "Explore Minion Browser By Minion Type";
			public const string BGHeroPickOverlay = "Support Best Hero Choice";
			public const string BGQuestStatsOverlay = "Support Best Quest/Reward Choice";

			// Mercenaries
			public const string MercOpponentAbilities = "Hover Opponent Merc Abilities";
			public const string MercFriendlyTasks = "Hover Friendly Merc Tasks";
		}

		public enum VMKind
		{
			Free,
			Paid,
		}


		public ValueMoment(string name, bool isPaid, int maxValueMomentCount = 1) : this(name, isPaid ? VMKind.Paid : VMKind.Free, maxValueMomentCount)
		{
		}

		public ValueMoment(string name, VMKind kind, int maxValueMomentCount = 1)
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
