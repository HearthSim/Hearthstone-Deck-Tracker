using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for DeckOptions.xaml
	/// </summary>
	public partial class DeckOptions
	{
		//TODO: Convert this into a Flyout with a user control inside of it!!!
		
		public delegate void DeckOptionsButtonClickedEvent(DeckOptions sender);

		public DeckOptions()
		{
			InitializeComponent();
		}

		public event DeckOptionsButtonClickedEvent DeckOptionsButtonClicked;

		private void After_Click()
		{
			if (DeckOptionsButtonClicked != null)
				DeckOptionsButtonClicked(this);
		}


		private async void BtnExport_Click(object sender, RoutedEventArgs e)
		{
			var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
			if (deck == null) return;

			var result = await Helper.MainWindow.ShowMessageAsync("Export " + deck.Name + " to Hearthstone",
			                                                      "Please create a new, empty " + deck.Class +
			                                                      "-Deck in Hearthstone before continuing (leave the deck creation screen open).\nDo not move your mouse after clicking OK!",
			                                                      MessageDialogStyle.AffirmativeAndNegative);

			if (result == MessageDialogResult.Affirmative)
			{
				var controller =
					await Helper.MainWindow.ShowProgressAsync("Creating Deck", "Please do not move your mouse or type.");
				Helper.MainWindow.Topmost = false;
				await Task.Delay(500);
				await DeckExporter.Export(Helper.MainWindow.DeckPickerList.SelectedDeck);
				await controller.CloseAsync();
			}

			After_Click();
		}

		private async void BtnScreenhot_Click(object sender, RoutedEventArgs e)
		{
			if (Helper.MainWindow.DeckPickerList.SelectedDeck == null) return;
			var screenShotWindow = new PlayerWindow(Config.Instance, Helper.MainWindow.DeckPickerList.SelectedDeck.Cards, true);
			screenShotWindow.Show();
			screenShotWindow.Top = 0;
			screenShotWindow.Left = 0;
			await Task.Delay(100);
			var source = PresentationSource.FromVisual(screenShotWindow);
			if (source == null) return;

			var dpiX = 96.0*source.CompositionTarget.TransformToDevice.M11;
			var dpiY = 96.0*source.CompositionTarget.TransformToDevice.M22;

			var fileName = Helper.ScreenshotDeck(screenShotWindow.ListViewPlayer, dpiX, dpiY,
			                                     Helper.MainWindow.DeckPickerList.SelectedDeck.Name);

			screenShotWindow.Shutdown();
			if (fileName == null)
			{
				await Helper.MainWindow.ShowMessageAsync("", "Error saving screenshot");
			}
			else
			{
				await Helper.MainWindow.ShowSavedFileMessage(fileName, "Screenshots");
			}

			After_Click();
		}

		private void BtnNotes_Click(object sender, RoutedEventArgs e)
		{
			if (Helper.MainWindow.DeckPickerList.SelectedDeck == null) return;
			Helper.MainWindow.FlyoutNotes.IsOpen = !Helper.MainWindow.FlyoutNotes.IsOpen;

			After_Click();
		}

		private async void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
		{
			var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
			if (deck != null)
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Yes";
				settings.NegativeButtonText = "No";
				var result =
					await
					Helper.MainWindow.ShowMessageAsync("Deleting " + deck.Name, "Are you Sure?",
					                                   MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result == MessageDialogResult.Affirmative)
				{
					try
					{
						Helper.MainWindow.DeckList.DecksList.Remove(deck);
						Helper.MainWindow.WriteDecks();
						Helper.MainWindow.DeckPickerList.RemoveDeck(deck);
						Helper.MainWindow.ListViewDeck.ItemsSource = null;
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
			var clone = (Deck) Helper.MainWindow.DeckPickerList.SelectedDeck.Clone();

			while (Helper.MainWindow.DeckList.DecksList.Any(d => d.Name == clone.Name))
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Set";
				settings.DefaultText = clone.Name;
				var name =
					await
					Helper.MainWindow.ShowInputAsync("Name already exists",
					                                 "You already have a deck with that name, please select a different one.", settings);

				if (String.IsNullOrEmpty(name))
					return;

				clone.Name = name;
			}

			Helper.MainWindow.DeckList.DecksList.Add(clone);
			Helper.MainWindow.DeckPickerList.AddAndSelectDeck(clone);

			Helper.MainWindow.WriteDecks();

			After_Click();
		}

		private void BtnTags_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.FlyoutMyDecksSetTags.IsOpen = true;
			if (Helper.MainWindow.DeckPickerList.SelectedDeck != null)
				Helper.MainWindow.TagControlMyDecks.SetSelectedTags(Helper.MainWindow.DeckPickerList.SelectedDeck.Tags);

			After_Click();
		}

		private async void BtnSaveToFile_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
			if (deck == null) return;
			var path = Helper.GetValidFilePath("SavedDecks", deck.Name, ".xml");
			XmlManager<Deck>.Save(path, deck);
			await Helper.MainWindow.ShowSavedFileMessage(path, "SavedDecks");


			After_Click();
		}

		private void BtnClipboard_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
			if (deck == null) return;
			Clipboard.SetText(Helper.DeckToIdString(deck));
			Helper.MainWindow.ShowMessage("", "copied to clipboard");

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