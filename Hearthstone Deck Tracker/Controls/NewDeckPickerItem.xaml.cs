#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for NewDeckPickerItem.xaml
	/// </summary>
	public partial class NewDeckPickerItem : INotifyPropertyChanged
	{
		private const int Small = 36;
		private const int Big = 48;

		public NewDeckPickerItem()
		{
			InitializeComponent();
			Deck = DataContext as Deck;
		}

		public NewDeckPickerItem(Deck deck)
		{
			InitializeComponent();
			DataContext = deck;
			Deck = deck;
		}

		public Deck Deck { get; set; }
		public FontWeight SelectedFontWeight { get; private set; }
		public event PropertyChangedEventHandler PropertyChanged;

		public void OnSelected()
		{
			//BorderItem.Height = Big;
			SelectedFontWeight = FontWeights.Bold;
			OnPropertyChanged("SelectedFontWeight");
		}

		public void OnDelselected()
		{
			//BorderItem.Height = Small;
			SelectedFontWeight = FontWeights.Regular;
			OnPropertyChanged("SelectedFontWeight");
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
		{
			var activeDeck = DeckList.Instance.ActiveDeck;
			var selectedDecks = Helper.MainWindow.DeckPickerList.SelectedDecks;
			if(activeDeck == null || !selectedDecks.Any())
				return;
			Helper.MainWindow.TagControlEdit.SetSelectedTags(selectedDecks);
			MenuItemQuickSetTag.ItemsSource = Helper.MainWindow.TagControlEdit.Tags;
			MenuItemMoveDecktoArena.Visibility = activeDeck.IsArenaDeck ? Visibility.Collapsed : Visibility.Visible;
			MenuItemMoveDeckToConstructed.Visibility = activeDeck.IsArenaDeck ? Visibility.Visible : Visibility.Collapsed;
			MenuItemMissingCards.Visibility = activeDeck.MissingCards.Any() ? Visibility.Visible : Visibility.Collapsed;
			MenuItemUpdateDeck.Visibility = string.IsNullOrEmpty(activeDeck.Url) ? Visibility.Collapsed : Visibility.Visible;
			MenuItemOpenUrl.Visibility = string.IsNullOrEmpty(activeDeck.Url) ? Visibility.Collapsed : Visibility.Visible;
			MenuItemArchive.Visibility = selectedDecks.Any(d => !d.Archived) ? Visibility.Visible : Visibility.Collapsed;
			MenuItemUnarchive.Visibility = selectedDecks.Any(d => d.Archived) ? Visibility.Visible : Visibility.Collapsed;
			SeparatorDeck1.Visibility = string.IsNullOrEmpty(activeDeck.Url) && !activeDeck.MissingCards.Any()
				                            ? Visibility.Collapsed : Visibility.Visible;
		}

		private void BtnEditDeck_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnEditDeck_Click(sender, e);
		}

		private void BtnNotes_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnNotes_Click(sender, e);
		}

		private void BtnTags_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnTags_Click(sender, e);
		}

		private void BtnMoveDeckToArena_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnMoveDeckToArena_Click(sender, e);
		}

		private void BtnMoveDeckToConstructed_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnMoveDeckToConstructed_Click(sender, e);
		}

		private void MenuItemMissingDust_OnClick(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.MenuItemMissingDust_OnClick(sender, e);
		}

		private void BtnUpdateDeck_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnUpdateDeck_Click(sender, e);
		}

		private void BtnOpenDeckUrl_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnOpenDeckUrl_Click(sender, e);
		}

		private void BtnArchiveDeck_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnArchiveDeck_Click(sender, e);
		}

		private void BtnUnarchiveDeck_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnUnarchiveDeck_Click(sender, e);
		}

		private void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnDeleteDeck_Click(sender, e);
		}

		private void BtnCloneDeck_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnCloneDeck_Click(sender, e);
		}

		private void BtnCloneSelectedVersion_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnCloneSelectedVersion_Click(sender, e);
		}

		private void BtnName_Click(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.BtnName_Click(sender, e);
		}

		#region sorting properties

		public string Class
		{
			get { return Deck.GetClass; }
		}

		public DateTime LastEdited
		{
			get { return Deck.LastEdited; }
		}

		public double WinPercent
		{
			get { return Deck.WinPercent; }
		}

		public string DeckName
		{
			get { return Deck.Name; }
		}

		public string TagList
		{
			get { return Deck.TagList; }
		}

		public Visibility NoteVisibility
		{
			get { return string.IsNullOrEmpty(Deck.Note) ? Visibility.Collapsed : Visibility.Visible; }
		}

		public Visibility ArchivedVisibility
		{
			get { return Deck.Archived ? Visibility.Visible : Visibility.Collapsed; }
		}

		public string Note
		{
			get { return Deck.Note; }
		}

		#endregion
	}
}