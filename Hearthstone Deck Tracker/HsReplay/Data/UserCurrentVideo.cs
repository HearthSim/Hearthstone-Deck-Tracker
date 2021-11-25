using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.HsReplay.Data
{
	public class UserCurrentVideo
	{
		public UserCurrentVideo(string url, string language)
		{
			Url = url;
			Language = language;
		}

		public string Language { get; set; }

		public string Url { get; set; }
	}
}
