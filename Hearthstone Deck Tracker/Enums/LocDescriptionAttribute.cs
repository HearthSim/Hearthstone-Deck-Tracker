using System;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Enums
{
	public class LocDescriptionAttribute : Attribute
	{
		public string LocDescription { get; }
		public LocDescriptionAttribute(string key, bool upper = false)
		{
			LocDescription = LocUtil.Get(key, upper)?.Replace("\\n", Environment.NewLine);
		}
	}
}
