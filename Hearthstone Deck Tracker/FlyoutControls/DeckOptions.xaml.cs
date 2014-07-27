using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for DeckOptions.xaml
	/// </summary>
	public partial class DeckOptions : UserControl
	{
		//TODO: Convert this into a Flyout with a user control inside of it!!!

		public MainWindow Window;


		public event DeckOptionsButtonClickedEvent DeckOptionsButtonClicked;
		public delegate void DeckOptionsButtonClickedEvent(DeckOptions sender);
		public DeckOptions()
		{
			InitializeComponent();
		}

		private void After_Click()
		{
			if (DeckOptionsButtonClicked != null)
				DeckOptionsButtonClicked(this);
		}


		private async void BtnExport_Click(object sender, RoutedEventArgs e)
		{
			var deck = Window.DeckPickerList.SelectedDeck;
			if (deck == null) return;

			var result = await Window.ShowMessageAsync("Export " + deck.Name + " to Hearthstone",
											   "Please create a new, empty " + deck.Class + "-Deck in Hearthstone before continuing (leave the deck creation screen open).\nDo not move your mouse after clicking OK!",
											   MessageDialogStyle.AffirmativeAndNegative);

			if (result == MessageDialogResult.Affirmative)
			{
				var controller = await Window.ShowProgressAsync("Creating Deck", "Please do not move your mouse or type.");
				Window.Topmost = false;
				await Task.Delay(500);
				await DeckExporter.Export(Window.DeckPickerList.SelectedDeck);
				await controller.CloseAsync();
			}

			After_Click();
		}

		private async void BtnScreenhot_Click(object sender, RoutedEventArgs e)
		{
			if (Window.DeckPickerList.SelectedDeck == null) return;
			PlayerWindow screenShotWindow = new PlayerWindow(Config.Instance, Window.DeckPickerList.SelectedDeck.Cards, true);
			screenShotWindow.Show();
			screenShotWindow.Top = 0;
			screenShotWindow.Left = 0;
			await Task.Delay(100);
			PresentationSource source = PresentationSource.FromVisual(screenShotWindow);
			if (source == null) return;

			double dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
			double dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

			var fileName = Helper.ScreenshotDeck(screenShotWindow.ListViewPlayer, dpiX, dpiY, Window.DeckPickerList.SelectedDeck.Name);

			screenShotWindow.Shutdown();
			if (fileName == null)
			{
				await Window.ShowMessageAsync("", "Error saving screenshot");
			}
			else
			{
				await Window.ShowSavedFileMessage(fileName, "Screenshots");
			}

			After_Click();
		}

		private void BtnNotes_Click(object sender, RoutedEventArgs e)
		{
			if (Window.DeckPickerList.SelectedDeck == null) return;
			Window.FlyoutNotes.IsOpen = !Window.FlyoutNotes.IsOpen;

			After_Click();
		}

		private async void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
		{
			var deck = Window.DeckPickerList.SelectedDeck;
			if (deck != null)
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Yes";
				settings.NegativeButtonText = "No";
				var result = await Window.ShowMessageAsync("Deleting " + deck.Name, "Are you Sure?", MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result == MessageDialogResult.Affirmative)
				{
					try
					{
						Window._deckList.DecksList.Remove(deck);
						Window.WriteDecks();
						Window.DeckPickerList.RemoveDeck(deck);
						Window.ListViewDeck.ItemsSource = null;
					}
					catch (Exception)
					{
						Logger.WriteLine("Error deleting deck");
					}
				}
			}


			After_Click();
		}

		private async void BtnCloneDeck_Click(object sender, RoutedEventArgs e)
		{
			var clone = (Deck)Window.DeckPickerList.SelectedDeck.Clone();

			while (Window._deckList.DecksList.Any(d => d.Name == clone.Name))
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Set";
				settings.DefaultText = clone.Name;
				string name = await Window.ShowInputAsync("Name already exists", "You already have a deck with that name, please select a different one.", settings);

				if (String.IsNullOrEmpty(name))
					return;

				clone.Name = name;
			}

			Window._deckList.DecksList.Add(clone);
			Window.DeckPickerList.AddAndSelectDeck(clone);

			Window.WriteDecks();

			After_Click();
		}

		private void BtnTags_Click(object sender, RoutedEventArgs e)
		{
			Window.FlyoutMyDecksSetTags.IsOpen = true;
			if (Window.DeckPickerList.SelectedDeck != null)
				Window.TagControlMyDecks.SetSelectedTags(Window.DeckPickerList.SelectedDeck.Tags);

			After_Click();
		}

		private async void BtnSaveToFile_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = Window.DeckPickerList.SelectedDeck;
			if (deck == null) return;
			var path = Helper.GetValidFilePath("SavedDecks", deck.Name, ".xml");
			XmlManager<Deck>.Save(path, deck);
			await Window.ShowSavedFileMessage(path, "SavedDecks");


			After_Click();
		}

		private void BtnClipboard_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = Window.DeckPickerList.SelectedDeck;
			if (deck == null) return;
			Clipboard.SetText(Helper.DeckToIdString(deck));
			Window.ShowMessage("", "copied to clipboard");

			After_Click();
		}

		public void EnableButtons(bool enable)
		{
			BtnScreenshot.IsEnabled = enable;
			BtnNotes.IsEnabled = enable;
			BtnExportHs.IsEnabled = enable;
			BtnDeleteDeck.IsEnabled = enable;
			BtnCloneDeck.IsEnabled = enable;
			BtnTags.IsEnabled = enable;
			BtnSaveToFile.IsEnabled = enable;
			BtnClipboard.IsEnabled = enable;
		}
	}
}
