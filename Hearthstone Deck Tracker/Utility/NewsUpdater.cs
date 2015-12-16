using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.HearthStats.API;

namespace Hearthstone_Deck_Tracker.Utility
{
    internal static class NewsUpdater
    {
        private const int NewsCheckInterval = 300;
        private const int NewsTickerUpdateInterval = 30;
        private const int HearthStatsAutoSyncInterval = 300;
        private static string _currentNewsLine;
        private static DateTime _lastHearthStatsSync;
        private static DateTime _lastNewsCheck;
        private static DateTime _lastNewsUpdate;
        private static bool _update;
        private static string[] _news;
        private static int _newsLine;
        public static int CurrentNewsId { get; set; }

        private static void UpdateNews(int newsLine)
        {
            if (newsLine < _news.Length && _currentNewsLine != _news[newsLine])
            {
                _currentNewsLine = _news[newsLine];
                Core.MainWindow.NewsContentControl.Content = StringToTextBlock(_currentNewsLine);
            }
            Core.MainWindow.StatusBarItemNewsIndex.Content = string.Format("({0}/{1})", _newsLine + 1, _news.Length);
            _lastNewsUpdate = DateTime.Now;
        }

        private static void UpdateNews()
        {
            if (_news == null || _news.Length == 0)
                return;
            _newsLine++;
            if (_newsLine > _news.Length - 1)
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
            var rest = url.Success ? text.Split(new[] { (url.Value) }, StringSplitOptions.None) : new[] { text };
            if (rest.Length == 1)
                tb.Inlines.Add(rest[0]);
            else
            {
                for (int restIndex = 0, urlIndex = 0; restIndex < rest.Length; restIndex += 2, urlIndex++)
                {
                    ParseMarkup(rest[restIndex], tb);
                    var link = new Hyperlink();
                    link.NavigateUri = new Uri(url.Groups["url"].Value);
                    link.RequestNavigate += (sender, args) => Process.Start(args.Uri.AbsoluteUri);
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
            if (_newsLine < 0)
                _newsLine = _news.Length - 1;
            UpdateNews(_newsLine);
        }

        public static void NextNewsItem()
        {
            _newsLine++;
            if (_newsLine > _news.Length - 1)
                _newsLine = 0;
            UpdateNews(_newsLine);
        }

        internal static async void UpdateAsync()
        {
            const string url = "https://raw.githubusercontent.com/Epix37/HDT-Data/master/news";
            _update = true;
            _lastNewsCheck = DateTime.MinValue;
            _lastNewsUpdate = DateTime.MinValue;
            CurrentNewsId = Config.Instance.IgnoreNewsId;
            _lastHearthStatsSync = DateTime.Now;
            while (_update)
            {
                if ((DateTime.Now - _lastNewsCheck) > TimeSpan.FromSeconds(NewsCheckInterval))
                {
                    try
                    {
                        var oldNewsId = CurrentNewsId;
                        using (var client = new WebClient())
                        {
                            var raw = await client.DownloadStringTaskAsync(url);
                            var content =
                                raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(
                                    x => x.Trim()).ToArray();
                            try
                            {
                                CurrentNewsId = int.Parse(content[0].Split(':')[1].Trim());
                            }
                            catch (Exception)
                            {
                                CurrentNewsId = 0;
                            }
                            _news = content.Skip(1).ToArray();
                        }
                        if (CurrentNewsId > oldNewsId
                            || Core.MainWindow.StatusBarNews.Visibility == Visibility.Collapsed
                            && CurrentNewsId > Config.Instance.IgnoreNewsId)
                        {
                            Core.MainWindow.TopRow.Height = new GridLength(26);
                            Core.MainWindow.StatusBarNews.Visibility = Visibility.Visible;
                            Core.MainWindow.MinHeight += Core.MainWindow.StatusBarNewsHeight;
                            UpdateNews(0);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.WriteLine("Error loading news: " + e, "UpdateNews");
                    }
                    _lastNewsCheck = DateTime.Now;
                }
                if ((DateTime.Now - _lastNewsUpdate) > TimeSpan.FromSeconds(NewsTickerUpdateInterval))
                    UpdateNews();

                if (HearthStatsAPI.IsLoggedIn && Config.Instance.HearthStatsAutoSyncInBackground
                    && (DateTime.Now - _lastHearthStatsSync) > TimeSpan.FromSeconds(HearthStatsAutoSyncInterval))
                {
                    _lastHearthStatsSync = DateTime.Now;
                    HearthStatsManager.SyncAsync(background: true);
                }
                await Task.Delay(1000);
            }
        }
    }
}