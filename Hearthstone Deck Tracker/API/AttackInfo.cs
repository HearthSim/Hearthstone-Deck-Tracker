using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.API
{
	public class AttackInfo
	{
		public Card Attacker { get; }
		public Card Defender { get; }

		public AttackInfo(Card attacker, Card defender)
		{
			Attacker = attacker;
			Defender = defender;
		}
	}
}
