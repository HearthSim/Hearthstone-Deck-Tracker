#region

using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Other
{
	/// <summary>
	/// Interaction logic for OtherImporting.xaml
	/// </summary>
	public partial class OtherImporting
	{
		private bool _initialized;

		public OtherImporting()
		{
			InitializeComponent();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
		}

		public void Load()
		{
			CheckboxTagOnImport.IsChecked = Config.Instance.TagDecksOnImport;
			CheckboxImportNetDeck.IsChecked = Config.Instance.NetDeckClipboardCheck ?? false;
			CheckboxAutoSaveOnImport.IsChecked = Config.Instance.AutoSaveOnImport;
			_initialized = true;
		}

		private void CheckboxTagOnImport_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TagDecksOnImport = true;
			Config.Save();
		}

		private void CheckboxTagOnImport_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TagDecksOnImport = false;
			Config.Save();
		}

		private void CheckboxImportNetDeck_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.NetDeckClipboardCheck = true;
			Config.Save();
		}

		private void CheckboxImportNetDeck_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.NetDeckClipboardCheck = false;
			Config.Save();
		}

		private void CheckboxAutoSaveOnImport_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSaveOnImport = true;
			Config.Save();
		}

		private void CheckboxAutoSaveOnImport_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSaveOnImport = false;
			Config.Save();
		}

		private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			await
				Helper.MainWindow.ShowMessageAsync("How this works:",
				                                   "1) Build your arena deck (or enter the arena screen if you're done already)\n\n2) Leave the arena screen (go back to the main menu)\n\n3) Press \"IMPORT > FROM GAME: ARENA\"\n\n4) Adjust the numbers\n\nWhy the last step? Because this is not perfect. It is only detectable which cards are in the deck but NOT how many of each. You can increase the count of a card by just right clicking it.");
		}
	}
}