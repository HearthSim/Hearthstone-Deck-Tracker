using System;

namespace Hearthstone_Deck_Tracker.Utility.Attributes
{
	public class AssetNameAttribute : Attribute
	{
		public string Name { get; }

		public AssetNameAttribute(string name)
		{
			Name = name;
		}
	}
}
