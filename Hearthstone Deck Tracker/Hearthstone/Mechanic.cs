namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Mechanic
	{
		public Mechanic(string name, Deck deck)
		{
			Name = name;
			Count = deck.GetMechanicCount(name);
		}

		public string Name { get; }
		public int Count { get; }

		public string DisplayValue => $"{Name}: {Count}";
	}
}
