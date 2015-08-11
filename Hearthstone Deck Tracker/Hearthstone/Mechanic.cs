namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Mechanic
	{
		public Mechanic(string name, Deck deck)
		{
			Name = name;
			Count = deck.GetMechanicCount(name);
		}
		public string Name { get; private set; }
		public int Count { get; private set; }
		public string DisplayValue { get { return string.Format("{0}: {1}", Name, Count); } }
	}
}