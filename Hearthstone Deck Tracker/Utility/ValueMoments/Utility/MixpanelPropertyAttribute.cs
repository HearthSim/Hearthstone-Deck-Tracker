using System;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{

	[AttributeUsage(AttributeTargets.Field)]
	public class MixpanelPropertyAttribute : Attribute
	{
		public MixpanelPropertyAttribute(string name)
		{
			Name = name;
		}

		public string? Name { get; }
	}
}
