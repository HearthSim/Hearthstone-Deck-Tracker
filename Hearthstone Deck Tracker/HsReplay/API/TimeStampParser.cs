using System;
using System.Text.RegularExpressions;

namespace Hearthstone_Deck_Tracker.HsReplay.API
{
	internal class TimeStampParser
	{
		public TimeStampParser(DateTime startTime)
		{
			StartTime = startTime;
		}

		public DateTime StartTime { get; }

		public string Parse(string line)
		{
			if(StartTime == DateTime.MinValue)
				return line;
			var match = Regex.Match(line, @"^D ([\d:.]+) (.+)$");
			if(!match.Success)
				return line;
			var timeStamp = DateTime.Parse(match.Groups[1].Value);
			var date = StartTime.Date.AddTicks(timeStamp.Ticks - timeStamp.Date.Ticks);
			if(date < StartTime)
				date = date.AddDays(1);
			return $"D {date.ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz")} {match.Groups[2]}";
		}
	}
}
