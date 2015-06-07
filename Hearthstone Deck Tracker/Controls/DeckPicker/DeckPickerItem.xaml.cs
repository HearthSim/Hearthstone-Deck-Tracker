#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.DeckPicker.DeckPickerItemLayouts;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
	/// <summary>
	/// Interaction logic for DeckPickerItem.xaml
	/// </summary>
	public partial class DeckPickerItem : INotifyPropertyChanged
	{
		private static Type _deckPickerItem = typeof(DeckPickerItemLayout1);

		public DeckPickerItem()
		{
			InitializeComponent();
			Deck = DataContext as Deck;
			SetLayout();
		}

		public DeckPickerItem(Deck deck, Type deckPickerItemLayout)
		{
			InitializeComponent();
			DataContext = deck;
			Deck = deck;
			_deckPickerItem = deckPickerItemLayout;
			SetLayout();
		}

		public Deck Deck { get; set; }

		public FontWeight FontWeightActiveDeck
		{
			get { return Equals(Deck, DeckList.Instance.ActiveDeck) ? FontWeights.Bold : FontWeights.Regular; }
		}

		public FontWeight FontWeightSelected
		{
			get
			{
				return Equals(Deck, DeckList.Instance.ActiveDeck)
					       ? FontWeights.Bold
					       : (Helper.MainWindow.DeckPickerList.SelectedDecks.Contains(Deck) ? FontWeights.SemiBold : FontWeights.Regular);
			}
		}

		public string TextUseButton
		{
			get { return Deck.Equals(DeckList.Instance.ActiveDeck) ? "ACTIVE" : "USE"; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void SetLayout()
		{
			Content = Activator.CreateInstance(_deckPickerItem);
		}

		public void RefreshProperties()
		{
			OnPropertyChanged("FontWeightSelected");
			OnPropertyChanged("FontWeightActiveDeck");
			OnPropertyChanged("TextUseButton");
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
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

		#endregion
	}

	public class Command : ICommand
	{
		private readonly Action _action;

		public Command(Action action)
		{
			_action = action;
		}

		public bool CanExecute(object parameter)
		{
			return _action != null;
		}

		public void Execute(object parameter)
		{
			_action.Invoke();
		}

		public event EventHandler CanExecuteChanged;
	}
}