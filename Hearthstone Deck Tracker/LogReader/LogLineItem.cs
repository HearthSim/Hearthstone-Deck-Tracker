#region

using System;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogLineItem
	{
		public LogLineItem(string ns, string line)
		{
			Namespace = ns;
			Line = line;
			DateTime time;
			Time = (line.Length > 20 && DateTime.TryParse(Line.Substring(2, 16), out time)) ? DateTime.Today.Add(time.TimeOfDay) : DateTime.Now;
			if(Time > DateTime.Now)
				Time = Time.AddDays(-1);
		}

		public string Namespace { get; set; }
		public DateTime Time { get; }
		public string Line { get; set; }
	}
}