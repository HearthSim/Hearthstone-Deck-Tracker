#region

using System;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.LogConfig
{
	internal class RequiredValueAttribute : Attribute
	{
		public RequiredValueAttribute(object value = null)
		{
			Value = value;
		}

		public object Value { get; set; }
	}
}