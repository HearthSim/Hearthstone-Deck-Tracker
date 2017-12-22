#region

using System;
using System.Text.RegularExpressions;

#endregion

namespace HearthWatcher.LogReader
{
	public class LogLine
	{
		public LogLine(string ns, string line)
		{
			Namespace = ns;
			Line = line;
			var regex = new Regex("^(D|W) (?<ts>([\\d:.]+)) (?<line>(.*))$");
			var match = regex.Match(line);
			if(match.Success)
			{
				DateTime time;
				var ts = match.Groups["ts"].Value;
				if(DateTime.TryParse(ts, out time))
				{
					Time = DateTime.Today.Add(time.TimeOfDay);
					if(Time > DateTime.Now)
						Time = Time.AddDays(-1);
				}
				LineContent = match.Groups["line"].Value;
			}
		}

		public string Namespace { get; set; }
		public DateTime Time { get; } = DateTime.Now;
		public string Line { get; set; }
		public string LineContent { get; set; }
	}
}
