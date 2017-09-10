using System.Windows.Input;
using Hearthstone_Deck_Tracker.FlyoutControls.Options;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Information
{
	public partial class TwitchExtensionInfo
	{
		public TwitchExtensionInfo()
		{
			InitializeComponent();
		}

		public ICommand OptionsCommand => new Command(() =>
		{
			AdvancedOptions.Instance.Show = true;
			Core.MainWindow.Options.TreeViewItemStreamingTwitchExtension.IsSelected = true;
			Core.MainWindow.FlyoutOptions.IsOpen = true;
		});

		public ICommand SetupGuideCommand => new Command(() => Helper.TryOpenUrl("https://hsdecktracker.net/twitch/setup/"));
	}
}
