using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for DeckPickerItem.xaml
	/// </summary>
	public partial class DeckPickerItem : UserControl
	{
		public DeckPickerItem()
		{
			InitializeComponent();
		}

		public string DeckName
		{
			get { return GetDeckName(); }
		}

		public string Underline
		{
			get
			{
				var deck = DataContext as Deck;
				if(deck != null && deck.IsSelectedInGui)
					return "Underline";
				return "None";
			}
		}

		private string GetDeckName()
		{
			var hsClass = DataContext as DeckPicker.HsClass;
			if(hsClass != null)
				return hsClass.GetName;
			var deck = DataContext as Deck;
			if(deck != null)
				return deck.Name;
			return string.Empty;
		}
	}
}