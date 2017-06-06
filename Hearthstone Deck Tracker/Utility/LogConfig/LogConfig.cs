#region

using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.LogConfig
{
	internal class LogConfig
	{
		public bool Updated { get; private set; }

		public List<LogConfigItem> Items { get; } = new List<LogConfigItem>();

		public void Add(LogConfigItem configItem)
		{
			Log.Info($"Adding {configItem.Name}");
			Items.Add(configItem);
			Updated = true;
		}

		public void Verify()
		{
			foreach(var item in Items)
				Updated |= item.VerifyAndUpdate();
		}
	}
}
