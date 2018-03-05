using System.Windows.Input;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Controls.Information
{
	public partial class CollectionSyncingInfo
	{
		public CollectionSyncingInfo()
		{
			InitializeComponent();
		}

		public ICommand LoginCommand => new Command(() =>
		{
			Core.MainWindow.FlyoutUpdateNotes.IsOpen = false;
			Helper.OptionsMain.TreeViewItemHSReplayCollection.IsSelected = true;
			Core.MainWindow.FlyoutOptions.IsOpen = true;
			HSReplayNetHelper.TryAuthenticate().Forget();
		});

		public ICommand CloseCommand => new Command(() => Core.MainWindow.FlyoutUpdateNotes.IsOpen = false);
	}
}
