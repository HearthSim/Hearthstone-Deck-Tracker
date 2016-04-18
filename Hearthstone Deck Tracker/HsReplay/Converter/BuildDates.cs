#region

using System;
using System.Collections.Generic;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay.Converter
{
	internal static class BuildDates
	{
		private static readonly List<BuildDate> KnownBuildDates = new List<BuildDate>
		{
			{DateTime.Parse("April 14, 2016"), 12266},
			{DateTime.Parse("Mar 14, 2016"), 12051},
			{DateTime.Parse("Dec 4, 2015"), 10956},
			{DateTime.Parse("Nov 10, 2015"), 10833},
			{DateTime.Parse("Oct 20, 2015"), 10604},
			{DateTime.Parse("Sep 29, 2015"), 10357},
			{DateTime.Parse("Aug 18, 2015"), 9786},
			{DateTime.Parse("Jun 29, 2015"), 9554},
			{DateTime.Parse("Jun 15, 2015"), 9166},
			{DateTime.Parse("May 14, 2015"), 8834},
			{DateTime.Parse("Apr 14, 2015"), 8416},
			{DateTime.Parse("Mar 31, 2015"), 8311},
			{DateTime.Parse("Mar 19, 2015"), 8108},
			{DateTime.Parse("Feb 26, 2015"), 8036},
			{DateTime.Parse("Feb 25, 2015"), 7835},
			{DateTime.Parse("Feb 9, 2015"), 7785},
			{DateTime.Parse("Jan 29, 2015"), 7628},
			{DateTime.Parse("Dec 4, 2014"), 7234},
			{DateTime.Parse("Oct 29, 2014"), 6898},
			{DateTime.Parse("Sep 22, 2014"), 6485},
			{DateTime.Parse("Aug 16, 2014"), 6284},
			{DateTime.Parse("Aug 6, 2014"), 6187},
			{DateTime.Parse("Jul 31, 2014"), 6141},
			{DateTime.Parse("Jul 22, 2014"), 6024},
			{DateTime.Parse("Jun 30, 2014"), 5834},
			{DateTime.Parse("May 28, 2014"), 5506},
			{DateTime.Parse("May 21, 2014"), 5435},
			{DateTime.Parse("May 8, 2014"), 5314},
			{DateTime.Parse("Apr 10, 2014"), 5170},
			{DateTime.Parse("Mar 13, 2014"), 4973},
			{DateTime.Parse("Jan 17, 2014"), 4482},
			{DateTime.Parse("Jan 16, 2014"), 4458},
			{DateTime.Parse("Jan 13, 2013"), 4442},
			{DateTime.Parse("Dec 10, 2013"), 4217},
			{DateTime.Parse("Oct 17, 2013"), 3937},
			{DateTime.Parse("Oct 2, 2013"), 3890},
			{DateTime.Parse("Aug 14, 2013"), 3664},
			{DateTime.Parse("Aug 13, 2013"), 3645},
			{DateTime.Parse("Aug 12, 2013"), 3604},
			{DateTime.Parse("Jun 22, 2013"), 3388},
			{DateTime.Parse("Jun 5, 2013"), 3140}
		};

		public static int? GetByDate(DateTime date)
		{
			foreach(var buildDate in KnownBuildDates)
			{
				if(date >= buildDate.Date)
					return buildDate.Build;
			}
			return null;
		}
	}

	public static class BuildDateListExtension
	{
		public static void Add(this List<BuildDate> list, DateTime buildDate, int build)
			=> list.Add(new BuildDate {Build = build, Date = buildDate});
	}

	public class BuildDate
	{
		public int Build { get; set; }
		public DateTime Date { get; set; }
	}
}