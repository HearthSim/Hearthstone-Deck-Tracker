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


		private void BtnExport_Click(object sender, RoutedEventArgs e)
		{
			After_Click();
		}

		private async void BtnScreenhot_Click(object sender, RoutedEventArgs e)
		{
			if (Window.DeckPickerList.SelectedDeck == null) return;
			PlayerWindow screenShotWindow = new PlayerWindow(Window._config, Window.DeckPickerList.SelectedDeck.Cards, true);
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

		private void BtnCloneDeck_Click(object sender, RoutedEventArgs e)
		{
			After_Click();
		}

		private void BtnTags_Click(object sender, RoutedEventArgs e)
		{
			After_Click();
		}

		private void BtnSaveToFile_OnClick(object sender, RoutedEventArgs e)
		{
			After_Click();
		}

		private void BtnClipboard_OnClick(object sender, RoutedEventArgs e)
		{
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
