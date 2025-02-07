#region

using System.Windows;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for Help.xaml
	/// </summary>
	public partial class Help
	{
		public Help()
		{
			InitializeComponent();
		}

		public string VersionString => Helper.GetCurrentVersion().ToVersionString();

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) => Helper.TryOpenUrl(e.Uri.AbsoluteUri);

		private void ButtonUpdateNotes_OnClick(object sender, RoutedEventArgs e)
		{
			if(this.ParentMainWindow() is not { } window)
				return;
			window.UpdateNotesControl.LoadReleaseNotes();
			window.FlyoutHelp.IsOpen = false;
			window.FlyoutUpdateNotes.IsOpen = true;
		}
	}
}
