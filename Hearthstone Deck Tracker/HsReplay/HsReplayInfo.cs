using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public class HsReplayInfo
	{
		public HsReplayInfo()
		{
			
		}

		public HsReplayInfo(string id)
		{
			Id = id;
		}

		public string Id { get; set; }

		public bool Uploaded => !string.IsNullOrEmpty(Id);

		public string Url => $"{Constants.BaseUrl}/joust/replay/{Id}";
	}
}
