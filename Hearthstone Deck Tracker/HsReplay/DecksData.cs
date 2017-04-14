using System;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public class DecksData
	{
		public DateTime ServerTimeStamp { get; set; }

		public DateTime ClientTimeStamp { get; set; }

		public string[] Decks { get; set; } = new string[0];

		[JsonIgnore]
		public TimeSpan Age => DateTime.Now - ClientTimeStamp;

		[JsonIgnore]
		public bool IsStale => Age.TotalHours > 24;

		public override string ToString()
		{
			return $"Count={Decks.Length}, ServerTS={ServerTimeStamp}, Downloaded={ClientTimeStamp} Age={Age}";
		}
	}
}