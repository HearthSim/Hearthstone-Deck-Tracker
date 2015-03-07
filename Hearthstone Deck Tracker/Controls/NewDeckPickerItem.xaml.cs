﻿#region

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
		private FontWeight _fontWeight;

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
			get { return _fontWeight; }
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
			get { return Deck.WinPercentAll; }
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

		public string Note
		{
			get { return Deck.Note; }
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnSelected()
		{
			//BorderItem.Height = Big;
			_fontWeight = FontWeights.Bold;
			OnPropertyChanged("SelectedFontWeight");
		}

		public void OnDelselected()
		{
			//BorderItem.Height = Small;
			_fontWeight = FontWeights.Regular;
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
			var deck = DeckList.Instance.ActiveDeck;
			MenuItemQuickSetTag.ItemsSource = Helper.MainWindow.TagControlEdit.Tags;
			MenuItemMoveDecktoArena.Visibility = deck.IsArenaDeck ? Visibility.Collapsed : Visibility.Visible;
			MenuItemMoveDeckToConstructed.Visibility = deck.IsArenaDeck ? Visibility.Visible : Visibility.Collapsed;
			MenuItemMissingCards.Visibility = deck.MissingCards.Any() ? Visibility.Visible : Visibility.Collapsed;
			MenuItemUpdateDeck.Visibility = string.IsNullOrEmpty(deck.Url) ? Visibility.Collapsed : Visibility.Visible;
			MenuItemOpenUrl.Visibility = string.IsNullOrEmpty(deck.Url) ? Visibility.Collapsed : Visibility.Visible;
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
	}
}