#region

using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	internal static class NewsManager
	{
		private const int NewsTickerUpdateInterval = 15;
		private static string _currentNewsLine;
		private static DateTime _lastNewsUpdate = DateTime.MinValue;
		private static int _newsLine;
		private static NewsData _news;
		private static bool _updating;

		private static void UpdateNews(int newsLine)
		{
			if(_news == null || _news.Data.Length == 0)
				return;
			if(newsLine < _news.Data.Length && _currentNewsLine != _news.Data[newsLine])
			{
				_currentNewsLine = _news.Data[newsLine];
				Core.MainWindow.NewsBar.NewsContent = StringToTextBlock(_currentNewsLine);
			}
			Core.MainWindow.NewsBar.IndexContent = $"({_newsLine + 1}/{_news.Data.Length})";
			_lastNewsUpdate = DateTime.Now;
		}

		private static void UpdateNews()
		{
			if(_news == null || _news.Data.Length == 0)
				return;
			_newsLine++;
			if(_newsLine > _news.Data.Length - 1)
				_newsLine = 0;
			UpdateNews(_newsLine);
		}

		private static TextBlock StringToTextBlock(string text)
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

		public static void PreviousNewsItem()
		{
			_newsLine--;
			if(_newsLine < 0)
				_newsLine = _news.Data.Length - 1;
			UpdateNews(_newsLine);
		}

		public static void NextNewsItem()
		{
			_newsLine++;
			if(_newsLine > _news.Data.Length - 1)
				_newsLine = 0;
			UpdateNews(_newsLine);
		}

		public static void ToggleNewsVisibility()
		{
			if(Core.MainWindow.NewsBar.Visibility == Visibility.Collapsed)
			{
				Config.Instance.IgnoreNewsId = -1;
				Config.Save();
				ShowNewsBar();
				if(!_updating)
					UpdateNewsAsync();
			}
			else
			{
				Config.Instance.IgnoreNewsId = _news?.Id ?? 0;
				Config.Save();
				Core.MainWindow.NewsBar.Visibility = Visibility.Collapsed;
				Core.MainWindow.MinHeight -= Core.MainWindow.StatusBarNewsHeight;
			}
		}

		internal static async void LoadNews()
		{
			try
			{
				using(var client = new WebClient())
				{
					var json = await client.DownloadStringTaskAsync("https://hsdecktracker.net/news.json");
					_news = JsonConvert.DeserializeObject<NewsData>(json);
					if(_news.Id > Config.Instance.IgnoreNewsId)
					{
						ShowNewsBar();
						UpdateNewsAsync();
					}
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private static async void UpdateNewsAsync()
		{
			if(_news != null && _news.Data.Length <= 1)
				return;
			_updating = true;
			while(true)
			{
				await Task.Delay(10000);
				if((DateTime.Now - _lastNewsUpdate) > TimeSpan.FromSeconds(NewsTickerUpdateInterval))
					UpdateNews();
			}
		}

		private static void ShowNewsBar()
		{
			Core.MainWindow.NewsBar.Visibility = Visibility.Visible;
			Core.MainWindow.MinHeight += Core.MainWindow.StatusBarNewsHeight;
			UpdateNews(0);
		}

		internal class NewsData
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("data")]
			public string[] Data { get; set; }
		}
	}
}