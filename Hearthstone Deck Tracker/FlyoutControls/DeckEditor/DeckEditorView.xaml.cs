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
		}

		private DeckEditorViewModel GetViewModel()
		{
			if(DataContext != null)
				return (DeckEditorViewModel)DataContext;

			var viewModel = new DeckEditorViewModel();
			
			viewModel.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == "Deck")
				{
					var deck = viewModel.Deck;
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

			DataContext = viewModel;
			return viewModel;
		}

		public void SetDeck(Deck deck, bool isNewDeck) => GetViewModel().SetDeck(deck, isNewDeck);

		public void SetCards(IEnumerable<Card> cards) => GetViewModel().SetCards(cards);

		public Deck CurrentDeck => GetViewModel().Deck;

		public void SetDeckName(string name) => GetViewModel().DeckName = name;
	}
}
