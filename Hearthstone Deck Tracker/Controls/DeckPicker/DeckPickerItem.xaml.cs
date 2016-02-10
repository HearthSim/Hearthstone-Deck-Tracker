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

		public FontWeight FontWeightActiveDeck => Equals(Deck, DeckList.Instance.ActiveDeck) ? FontWeights.Bold : FontWeights.Regular;

		public FontWeight FontWeightSelected => Equals(Deck, DeckList.Instance.ActiveDeck)
													? FontWeights.Bold
													: (Core.MainWindow.DeckPickerList.SelectedDecks.Contains(Deck) ? FontWeights.SemiBold : FontWeights.Regular);

		public string TextUseButton => Deck.Equals(DeckList.Instance.ActiveDeck) ? "ACTIVE" : "USE";

		public event PropertyChangedEventHandler PropertyChanged;

		public void SetLayout() => Content = Activator.CreateInstance(_deckPickerItem);

		public void RefreshProperties()
		{
			OnPropertyChanged(nameof(FontWeightSelected));
			OnPropertyChanged(nameof(FontWeightActiveDeck));
			OnPropertyChanged(nameof(TextUseButton));
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region sorting properties

		public string Class => Deck.GetClass;

		public DateTime LastEdited => Deck.LastEdited;

		public DateTime LastPlayed => Deck.LastPlayed;

		public DateTime LastPlayedNewFirst => Deck.LastPlayedNewFirst;

		public double WinPercent => Deck.WinPercent;

		public string DeckName => Deck.Name;

		public string TagList => Deck.TagList;

		#endregion
	}

	public class Command : ICommand
	{
		private readonly Action _action;

		public Command(Action action)
		{
			_action = action;
		}

		public bool CanExecute(object parameter) => _action != null;

		public void Execute(object parameter) => _action.Invoke();
#pragma warning disable 0067
		public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
	}
}