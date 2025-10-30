#region

using System;
using System.Text.RegularExpressions;

#endregion

namespace HearthWatcher.LogReader;

public class LogLine
{
	private static readonly Regex _regex = new(
		"^(D|W) (?<ts>([\\d:.]+)) (?<line>(.*))$",
		options: RegexOptions.Compiled
	);

	public LogLine(string ns, string line)
	{
		line = line.Replace("\n", "").Trim();
		Namespace = ns;
		Line = line;
		var match = _regex.Match(line);
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

	public string Namespace { get; }
	public DateTime Time { get; } = DateTime.Now;
	public string Line { get; }
	public string LineContent { get; }
}
