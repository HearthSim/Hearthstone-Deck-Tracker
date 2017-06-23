using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckEditor
{
	/// <summary>
	/// Interaction logic for DeckEditorView.xaml
	/// </summary>
	public partial class DeckEditorView : UserControl
	{

		public DeckEditorView()
		{
			InitializeComponent();
			var viewModel = ((DeckEditorViewModel)DataContext);
			
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
	}
}
