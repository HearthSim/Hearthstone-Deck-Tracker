#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
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

		public FontWeight SelectedFontWeight
		{
			get { return Equals(Deck, DeckList.Instance.ActiveDeck) ? FontWeights.Bold : FontWeights.Regular; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void RefreshProperties()
		{
			OnPropertyChanged("SelectedFontWeight");
			OnPropertyChanged("TextUseButton");
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void UseButton_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if(!Deck.Equals(DeckList.Instance.ActiveDeck))
			{
				Helper.MainWindow.SelectDeck(Deck, true);
				Helper.MainWindow.DeckPickerList.RefreshDisplayedDecks();
			}
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

		public DateTime LastPlayed
		{
			get { return Deck.LastPlayed; }
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

		public string TextUseButton
		{
			get { return Deck.Equals(DeckList.Instance.ActiveDeck) ? "ACTIVE" : "USE"; }
		}

		#endregion
	}
}