#region

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Information;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	public partial class UpdateNotes : INotifyPropertyChanged
	{
		private bool _animateTransition;
		private bool _continueToHighlight;


		private bool _showHighlight;

		public UpdateNotes()
		{
			InitializeComponent();
		}

		private string? _releaseNotes;

		public string? ReleaseNotes
		{
			get => _releaseNotes;
			set
			{
				if(_releaseNotes == value)
					return;
				_releaseNotes = value;
				OnPropertyChanged();
			}
		}

		public bool ShowHighlight
		{
			get => _showHighlight;
			set
			{
				if(_showHighlight != value)
				{
					_showHighlight = value;
					OnPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void SetHighlight(Version? previousVersion)
		{
			if(previousVersion == null)
				return;
			UserControl? infoControl = null;
			if(previousVersion < new Version(0, 13, 18))
				infoControl = new CardThemesInfo();
#if(!SQUIRREL)
			if(previousVersion < new Version(0, 15, 14) && Config.Instance.SaveConfigInAppData != false
														&& Config.Instance.SaveDataInAppData != false)
			{
				ContentControlHighlight.Content = new SquirrelInfo();
				ButtonContinue.Visibility = Visibility.Collapsed;
				_continueToHighlight = true;
				_animateTransition = true;
				return;
			}
#endif
			if(previousVersion < new Version(1, 2, 4))
				infoControl = new HsReplayStatisticsInfo();

			if(infoControl == null)
				return;
			ContentControlHighlight.Content = infoControl;
			ShowHighlight = true;
		}

		public void LoadReleaseNotes()
		{
			if(_releaseNotes != null)
				return;
			try
			{
				using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Hearthstone_Deck_Tracker.Resources.CHANGELOG.md");
				using var reader = new StreamReader(stream);

				var releaseNotes = reader.ReadToEnd();
				releaseNotes = Regex.Replace(releaseNotes, "\\\\\r", "\r"); // yes, this the right number of slashes...
				releaseNotes = Regex.Replace(releaseNotes, "\n", "\n\n");
				releaseNotes = Regex.Replace(releaseNotes, "#(\\d+)",
					"[#$1](https://github.com/HearthSim/Hearthstone-Deck-Tracker/issues/$1)");

				ReleaseNotes = releaseNotes;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				ReleaseNotes = null;
			}
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ButtonClose_Click(object sender, RoutedEventArgs e)
		{
			if(this.ParentMainWindow() is not { } window)
				return;
			if(_continueToHighlight)
			{
				ShowHighlight = true;
				window.FlyoutUpdateNotes.IsModal = true;
				window.FlyoutUpdateNotes.TitleVisibility = Visibility.Collapsed;
				if(_animateTransition)
				{
					window.FlyoutUpdateNotes.BeginAnimation(HeightProperty, new DoubleAnimation(window.FlyoutUpdateNotes.ActualHeight, 400, TimeSpan.FromMilliseconds(250)));
				}
			}
			else
				window.FlyoutUpdateNotes.IsOpen = false;
		}

		private void ButtonContinue_OnClick(object sender, RoutedEventArgs e)
		{
			ShowHighlight = false;
		}

		private void ButtonHSReplaynet_Click(object sender, RoutedEventArgs e)
		{
			var url = Helper.BuildHsReplayNetUrl("premium", "updatenotes");
			if(!Helper.TryOpenUrl(url) && this.ParentMainWindow() is { } window)
				window.ShowMessage("Could not start your browser", "You can find our premium page at https://hsreplay.net/premium/").Forget();
		}
	}
}
