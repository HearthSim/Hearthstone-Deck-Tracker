#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Information;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for UpdateNotes.xaml
	/// </summary>
	public partial class UpdateNotes : INotifyPropertyChanged
	{
		private List<GithubRelease> _fullReleaseNotes;
		private DateTime _lastExpand = DateTime.MinValue;
		private int _numVersions = 3;
		private bool _continueToHighlight;

		public UpdateNotes()
		{
			InitializeComponent();
			_fullReleaseNotes = new List<GithubRelease>();
		}

		public void SetHighlight(Version previousVersion)
		{
			if(previousVersion == null)
				return;
			UserControl infoControl = null;
			if(previousVersion < new Version(0, 13, 18))
				infoControl = new CardThemesInfo();
#if(!SQUIRREL)
			if(previousVersion < new Version(0, 15, 14) && Config.Instance.SaveConfigInAppData != false && Config.Instance.SaveDataInAppData != false)
			{
				ContentControlHighlight.Content = new SquirrelInfo();
				ButtonContinue.Visibility = Visibility.Collapsed;
				_continueToHighlight = true;
				return;
			}
#endif
			if(infoControl == null)
				return;
			ContentControlHighlight.Content = infoControl;
			TabControl.SelectedIndex = 1;
		}

		private SerializableVersion CurrentVersion => new SerializableVersion(Helper.GetCurrentVersion());

		public List<GithubRelease> ReleaseNotes
		{
			get
			{
				var upToInstalled = _fullReleaseNotes.SkipWhile(r => r.GetVersion() != CurrentVersion).ToList();
				return (upToInstalled.Any() ? upToInstalled : _fullReleaseNotes).Take(_numVersions).ToList();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public async void LoadUpdateNotes()
		{
			const string latestReleaseRequestUrl = @"https://api.github.com/repos/HearthSim/Hearthstone-Deck-Tracker/releases";

			try
			{
				string versionStr;
				using(var wc = new WebClient())
				{
					wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
					versionStr = await wc.DownloadStringTaskAsync(latestReleaseRequestUrl);
				}
				_fullReleaseNotes = JsonConvert.DeserializeObject<GithubRelease[]>(versionStr).ToList();
				OnPropertyChanged(nameof(ReleaseNotes));
			}
			catch(Exception)
			{
			}
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if(ScrollViewerNotes.ScrollableHeight < 1 || (DateTime.Now - _lastExpand) < TimeSpan.FromSeconds(1)
			   || _numVersions >= _fullReleaseNotes.Count)
				return;
			if(Math.Abs(ScrollViewerNotes.VerticalOffset - ScrollViewerNotes.ScrollableHeight) < 5)
			{
				_numVersions += 2;
				OnPropertyChanged(nameof(ReleaseNotes));
				_lastExpand = DateTime.Now;
			}
		}

		private void ButtonShowGithub_OnClick(object sender, RoutedEventArgs e)
		{
			const string url = "https://github.com/HearthSim/Hearthstone-Deck-Tracker/releases";
			if (!Helper.TryOpenUrl(url))
				Core.MainWindow.ShowMessage("Could not start browser", $"You can find the releases at \"{url}\"").Forget();
		}

		private void FlowDocumentScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			ScrollViewerNotes.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {RoutedEvent = MouseWheelEvent});
			e.Handled = true;
		}

		private void UpdateNotes_OnLoaded(object sender, RoutedEventArgs e)
		{
			_fullReleaseNotes = new List<GithubRelease>
			{
				new GithubRelease {Name = "Loading...", Body = "Loading...", TagName = CurrentVersion.ToString(true)}
			};
			OnPropertyChanged(nameof(ReleaseNotes));
		}

		private void ButtonPaypal_Click(object sender, RoutedEventArgs e)
		{
			if (!Helper.TryOpenUrl("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PZDMUT88NLFYJ"))
				Core.MainWindow.ShowMessage("Could not start browser", "You can also find a link at the bottom of the GitHub page!").Forget();
		}

		private void ButtonPatreon_Click(object sender, RoutedEventArgs e)
		{
			const string url = "https://www.patreon.com/HearthstoneDeckTracker";
			if (!Helper.TryOpenUrl(url))
				Core.MainWindow.ShowMessage("Could not start browser", "You can find the patreon page here: " + url).Forget();
		}

		private void ButtonClose_Click(object sender, RoutedEventArgs e)
		{
			if(_continueToHighlight)
			{
				TabControl.SelectedIndex = 1;
				Core.MainWindow.FlyoutUpdateNotes.Header = null;
				Core.MainWindow.FlyoutUpdateNotes.HeaderTemplate = null;
				Core.MainWindow.FlyoutUpdateNotes.BeginAnimation(HeightProperty,
					new DoubleAnimation(Core.MainWindow.FlyoutUpdateNotes.ActualHeight, 400, TimeSpan.FromMilliseconds(250)));
			}
			else
				Core.MainWindow.FlyoutUpdateNotes.IsOpen = false;
		}

		public class GithubRelease
		{
			private string _body;

			[JsonProperty("tag_name")]
			public string TagName { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("body")]
			public string Body
			{
				get { return _body; }
				set
				{
					_body = Regex.Replace(value, "\r\n", "\r\n\n");
					_body = Regex.Replace(_body, "#(\\d+)", "[#$1](https://github.com/HearthSim/Hearthstone-Deck-Tracker/issues/$1)");
				}
			}

			public SerializableVersion GetVersion() => SerializableVersion.ParseOrDefault(TagName);
		}

		private void ButtonContinue_OnClick(object sender, RoutedEventArgs e) => TabControl.SelectedIndex = 0;
	}
}