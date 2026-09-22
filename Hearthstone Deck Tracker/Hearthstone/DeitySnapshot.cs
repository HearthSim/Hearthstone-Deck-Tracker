namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class DeitySnapshot
	{
		public DeitySnapshot(Card card, int attack, int health, bool isGolden, int turn)
		{
			Card = card;
			Attack = attack;
			Health = health;
			IsGolden = isGolden;
			Turn = turn;
		}

		public Card Card { get; }
		public int Attack { get; }
		public int Health { get; }
		public bool IsGolden { get; }
		public int Turn { get; }
	}
}
