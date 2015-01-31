using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Hearthstone_Deck_Tracker.HearthStats
{
	/// <summary>
	/// Interaction logic for HearthStatsTestWindow.xaml
	/// </summary>
	public partial class HearthStatsTestWindow : Window
	{
		public HearthStatsTestWindow()
		{
			InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			var success = await HearthStatsAPI.Login(TextboxEmail.Text, TextboxPassword.Password);
			if(success)
			{
				TextboxEmail.IsEnabled = false;
				TextboxPassword.IsEnabled = false;
			}
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			HearthStatsAPI.PostDeck(Helper.MainWindow.DeckPickerList.GetSelectedDeckVersion());
		}

		private void BtnGetDecks_OnClick(object sender, RoutedEventArgs e)
		{
			HearthStatsAPI.GetDecks();
		}

		private void BtnPostGame_OnClick(object sender, RoutedEventArgs e)
		{
			HearthStatsAPI.PostGameResult(Helper.MainWindow.DeckPickerList.SelectedDeck);
		}
	}
}
