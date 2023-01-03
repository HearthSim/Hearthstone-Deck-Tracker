using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
