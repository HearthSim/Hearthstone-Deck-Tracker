#region

using System;
using System.ComponentModel;
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
	}
}