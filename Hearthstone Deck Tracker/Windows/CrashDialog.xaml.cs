using System;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using MahApps.Metro;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class CrashDialog
	{
		private readonly Exception _exception;
		private bool _hasClickedSend;

		public CrashDialog(Exception exception)
		{
			_exception = exception;
			InitializeComponent();
		}

		public string ExceptionMessage => _exception.Message;

		public string FullExceptionText => _exception.ToString();

		private void ButtonSend_Click(object sender, RoutedEventArgs e)
		{
			if(!string.IsNullOrEmpty(TextBoxDescription.Text))
				_exception.Data.Add("description", TextBoxDescription.Text);
			_hasClickedSend = true;
			Close();
		}

		private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();

		private void ButtonShowStacktrace_OnClick(object sender, RoutedEventArgs e)
		{
			if(TextBoxStackTrace.Visibility == Visibility.Collapsed)
			{
				TextBoxStackTrace.Visibility = Visibility.Visible;
				(TextBoxStackTrace.TryFindResource("ShowStackTraceStoryboard") as Storyboard)?.Begin();
				ButtonShowStacktrace.Content = "Hide Stacktrace";
			}
			else
			{
				TextBoxStackTrace.Visibility = Visibility.Collapsed;
				TextBoxStackTrace.Height = 0;
				(TextBoxStackTrace.TryFindResource("HideStackTraceStoryboard") as Storyboard)?.Begin();
				
				ButtonShowStacktrace.Content = "Show Stacktrace";
			}
		}

		private void CrashDialog_Closed(object sender, EventArgs e)
		{
			if(Config.Instance.GoogleAnalytics || _hasClickedSend)
				Sentry.CaptureException(_exception);
		}
	}
}
