namespace Hearthstone_Deck_Tracker.HsReplay.API
{
	internal class Header
	{
		public Header(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; }
		public string Value { get; }
	}
}