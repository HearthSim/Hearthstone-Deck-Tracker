#region

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for OtherImporting.xaml
	/// </summary>
	public partial class TrackerImporting
	{
		private bool _initialized;

		public TrackerImporting()
		{
			InitializeComponent();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
		}

		public void Load()
		{
			CheckBoxAutoDetectCardCount.IsChecked = Config.Instance.DeckImportAutoDetectCardCount;
			CheckboxTagOnImport.IsChecked = Config.Instance.TagDecksOnImport;
			CheckboxImportNetDeck.IsChecked = Config.Instance.NetDeckClipboardCheck ?? false;
			CheckboxAutoSaveOnImport.IsChecked = Config.Instance.AutoSaveOnImport;
			TextBoxArenaTemplate.Text = Config.Instance.ArenaDeckNameTemplate;
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

		private async void ButtonArenaHowTo_OnClick(object sender, RoutedEventArgs e)
		{
			await
				Helper.MainWindow.ShowMessageAsync("How this works:",
				                                   "1) Build your arena deck (or enter the arena screen if you're done already)\n\n2) Leave the arena screen (go back to the main menu)\n\n3) Press \"IMPORT > FROM GAME: ARENA\"\n\n4) Adjust the numbers\n\nWhy the last step? Because this is not perfect. It is only detectable which cards are in the deck but NOT how many of each. You can increase the count of a card by just right clicking it.");
		}

		private async void ButtonConstructedHowTo_OnClick(object sender, RoutedEventArgs e)
		{
			await
				Helper.MainWindow.ShowMessageAsync("How this works:",
				                                   "0) Build your deck\n\n1) Go to the main menu (always start from here!)\n\n2)Enter the collection and open the deck you want to import (do not edit the deck at this point)\n\n3)Leave the collection screen and go back to the main menu\n\n4) Press \"IMPORT > FROM GAME: CONSTRUCTED\"\n\n5) Adjust the numbers\n\nWhy the last step? Because this is not perfect. It is only detectable which cards are in the deck but NOT how many of each. Depening on what requires less clicks, non-legendary cards will default to 1 or 2.");
		}

		private void ButtonSetUpConstructed_OnClick(object sender, RoutedEventArgs e)
		{
			Helper.SetupConstructedImporting();
		}

		private void BtnEditTemplate_Click(object sender, RoutedEventArgs e)
		{
			if(TextBoxArenaTemplate.IsEnabled)
			{
				BtnEditTemplate.Content = "EDIT";
				Config.Instance.ArenaDeckNameTemplate = TextBoxArenaTemplate.Text;
				Config.Save();
				TextBoxArenaTemplate.IsEnabled = false;
			}
			else
			{
				BtnEditTemplate.Content = "SAVE";
				TextBoxArenaTemplate.IsEnabled = true;
			}
		}

		private void TextBoxArenaTemplate_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			TextBlockNamePreview.Text = Helper.ParseDeckNameTemplate(TextBoxArenaTemplate.Text);
		}

		private void ButtonActivateHdtProtocol_OnClick(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.SetupProtocol();
		}

		private void CheckBoxAutoDetectCardCount_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckImportAutoDetectCardCount = true;
			Config.Save();
		}

		private void CheckBoxAutoDetectCardCount_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckImportAutoDetectCardCount = false;
			Config.Save();
		}
	}
}