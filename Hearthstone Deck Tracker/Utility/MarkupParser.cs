using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class MarkupParser
	{
		public static TextBlock StringToTextBlock(string text)
		{
			var tb = new TextBlock();
			ParseMarkup(text, tb);
			return tb;
		}

		private static void ParseMarkup(string text, TextBlock tb)
		{
			const string urlMarkup = @"\[(?<text>(.*?))\]\((?<url>(http[s]?://.+\..+?))\)";

			var url = Regex.Match(text, urlMarkup);
			var rest = url.Success ? text.Split(new[] {(url.Value)}, StringSplitOptions.None) : new[] {text};
			if(rest.Length == 1)
				tb.Inlines.Add(rest[0]);
			else
			{
				for(var restIndex = 0; restIndex < rest.Length; restIndex += 2)
				{
					ParseMarkup(rest[restIndex], tb);
					var link = new Hyperlink();
					link.NavigateUri = new Uri(url.Groups["url"].Value);
					link.RequestNavigate += (sender, args) => Helper.TryOpenUrl(args.Uri.AbsoluteUri);
					link.Inlines.Add(new Run(url.Groups["text"].Value));
					link.Foreground = new SolidColorBrush(Colors.White);
					tb.Inlines.Add(link);
					ParseMarkup(rest[restIndex + 1], tb);
				}
			}
		}
	}
}