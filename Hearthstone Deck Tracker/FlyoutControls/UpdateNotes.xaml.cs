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
using Hearthstone_Deck_Tracker.Controls.Information;
using Hearthstone_Deck_Tracker.Properties;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using Microsoft.Win32;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for UpdateNotes.xaml
	/// </summary>
	public partial class UpdateNotes : INotifyPropertyChanged
	{
		private bool _continueToHighlight;

		public UpdateNotes()
		{
			InitializeComponent();
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
			if(previousVersion < new Version(1,2,4))
				infoControl = new HsReplayStatisticsInfo();
			if(previousVersion < new Version(1, 5, 2) && IsStreamingSoftwareInstalled())
				infoControl = new TwitchExtensionInfo();
			if(infoControl == null)
				return;
			ContentControlHighlight.Content = infoControl;
			TabControl.SelectedIndex = 1;
		}

		private bool IsStreamingSoftwareInstalled()
		{
			return HasRegistryEntry(@"SOFTWARE\OBS Studio") || HasRegistryEntry(@"SOFTWARE\SplitmediaLabs\XSplit");
		}

		private bool HasRegistryEntry(string key)
		{
			using(var obs = Registry.LocalMachine.OpenSubKey(key))
				return obs != null;
		}

		private string _releaseNotes;
		public string ReleaseNotes => _releaseNotes ?? (_releaseNotes = GetReleaseNotes());

		private string GetReleaseNotes()
		{
			try
			{
				string releaseNotes;
				using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Hearthstone_Deck_Tracker.Resources.CHANGELOG.md"))
				using(var reader = new StreamReader(stream))
					releaseNotes = reader.ReadToEnd();
				releaseNotes = Regex.Replace(releaseNotes, "\n", "\n\n");
				releaseNotes = Regex.Replace(releaseNotes, "#(\\d+)", "[#$1](https://github.com/HearthSim/Hearthstone-Deck-Tracker/issues/$1)");
				return releaseNotes;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

		private void ButtonContinue_OnClick(object sender, RoutedEventArgs e) => TabControl.SelectedIndex = 0;

		private void ButtonHSReplaynet_Click(object sender, RoutedEventArgs e)
		{
			var url = Helper.BuildHsReplayNetUrl("premium", "updatenotes");
			if (!Helper.TryOpenUrl(url))
				Core.MainWindow.ShowMessage("Could not start your browser", "You can find our premium page at https://hsreplay.net/premium/").Forget();
		}
	}
}
