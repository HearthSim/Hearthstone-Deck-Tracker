using System.Collections.Generic;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckEditor
{
	public partial class DeckEditorView
	{

		public DeckEditorView()
		{
			InitializeComponent();
			var viewModel = (DeckEditorViewModel)DataContext;
			
			viewModel.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == "Deck")
				{
					var deck = ((DeckEditorViewModel)DataContext).Deck;
					DeckSetIcons.Update(deck);
					ManaCurve.SetDeck(deck);
				}
			};

			viewModel.DbInputFocusRequest += () =>
			{
				TextBoxDbInput.Focus();
				Keyboard.Focus(TextBoxDbInput);
				TextBoxDbInput.SelectAll();
			};
		}

		public void SetDeck(Deck deck, bool isNewDeck) => ((DeckEditorViewModel)DataContext).SetDeck(deck, isNewDeck);

		public void SetCards(IEnumerable<Card> cards) => ((DeckEditorViewModel)DataContext).SetCards(cards);

		public Deck CurrentDeck => ((DeckEditorViewModel)DataContext).Deck;

		public void SetDeckName(string name) => ((DeckEditorViewModel)DataContext).DeckName = name;
	}
}
